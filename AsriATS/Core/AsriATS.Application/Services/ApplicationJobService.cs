using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.ApplicationJob;
using AsriATS.Application.DTOs;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsriATS.Application.DTOs.Email;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;
using AsriATS.Application.DTOs.Request;

namespace AsriATS.Application.Services
{
    public class ApplicationJobService : IApplicationJobService
    {
        private readonly IJobPostRequestRepository _jobPostRequestRepository;
        private readonly INextStepRuleRepository _nextStepRuleRepository;
        private readonly IWorkflowSequenceRepository _workflowSequenceRepository;
        private readonly IWorkflowRepository _workflowRepository;
        private readonly IProcessRepository _processRepository;
        private readonly IWorkflowActionRepository _workflowActionRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IJobPostRepository _jobPostRepository;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IApplicationJobRepository _applicationJobRepository;
        private readonly IDocumentSupportRepository _documentSupportRepository;

        public ApplicationJobService(IJobPostRequestRepository jobPostRequestRepository, INextStepRuleRepository nextStepRuleRepository, IWorkflowSequenceRepository workflowSequenceRepository, IWorkflowRepository workflowRepository, IProcessRepository processRepository, IWorkflowActionRepository workflowActionRepository, UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor, IJobPostRepository jobPostRepository, RoleManager<AppRole> roleManager, IEmailService emailService, IApplicationJobRepository applicationJobRepository, IDocumentSupportRepository documentSupportRepository)
        {
            _jobPostRequestRepository = jobPostRequestRepository;
            _nextStepRuleRepository = nextStepRuleRepository;
            _workflowSequenceRepository = workflowSequenceRepository;
            _workflowRepository = workflowRepository;
            _processRepository = processRepository;
            _workflowActionRepository = workflowActionRepository;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _jobPostRepository = jobPostRepository;
            _roleManager = roleManager;
            _emailService = emailService;
            _applicationJobRepository = applicationJobRepository;
            _documentSupportRepository = documentSupportRepository;
        }

        public async Task<BaseResponseDto> SubmitApplicationJob(ApplicationJobDto request, List<IFormFile> supportingDocuments = null)
        {
            // Get the current logged-in user
            var userName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "User not found!"
                };
            }

            // Validate the job post
            var jobPost = await _jobPostRepository.GetByIdAsync(request.JobPostId);
            if (jobPost == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Job post not found!"
                };
            }

            // Validate the process for job application workflow
            var workflow = await _workflowRepository.GetFirstOrDefaultAsync(w => w.WorkflowName == "Application Job Request");
            if (workflow == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Workflow for job application not found!"
                };
            }

            // Get the current step and next step in the workflow
            var currentStep = await _workflowSequenceRepository.GetFirstOrDefaultAsync(wfs => wfs.WorkflowId == workflow.WorkflowId);
            if (currentStep == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Current workflow step not found!"
                };
            }

            var nextStep = await _nextStepRuleRepository.GetFirstOrDefaultAsync(n => n.CurrentStepId == currentStep.StepId);
            if (nextStep == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Next step in the workflow not found!"
                };
            }

            // Create a new process for the application job
            var newProcess = new Process
            {
                RequesterId = user.Id,
                WorkflowId = workflow.WorkflowId,
                RequestType = "Application Job",
                Status = "Submitted",
                RequestDate = DateTime.UtcNow,
                CurrentStepId = nextStep.NextStepId
            };
            await _processRepository.CreateAsync(newProcess);

            // Create a new ApplicationJob entity
            var newApplicationJob = new ApplicationJob
            {
                Name = user.FirstName + " " + user.LastName,
                Email = user.Email,
                PhoneNumber = user.PhoneNumber,
                Address = user.Address,
                UserId = user.Id,
                WorkExperience = request.WorkExperience,
                Education = request.Education,
                Skills = request.Skills,
                JobPostId = request.JobPostId,
                ProcessId = newProcess.ProcessId,
                UploadedDate = DateTime.UtcNow,
                SupportingDocumentsId = request.SupportingDocumentsId,
                SupportingDocumentsIdNavigation = new List<SupportingDocument>() // Initialize the list of supporting documents
            };

            // Verify provided supporting documents are owned by the user
            if (request.SupportingDocumentsId != null)
            {
                var existingDocument = await _documentSupportRepository.GetByIdAsync(request.SupportingDocumentsId.Value);

                if (existingDocument == null || existingDocument.UserId != user.Id)
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = $"Invalid document ID {request.SupportingDocumentsId} or you do not own the document."
                    };
                }

                newApplicationJob.SupportingDocumentsIdNavigation.Add(existingDocument);
            }

            // Add additional new documents if provided
            if (supportingDocuments != null)
            {
                var uploadPath = Path.Combine(Directory.GetCurrentDirectory(), "uploads");
                if (!Directory.Exists(uploadPath))
                {
                    Directory.CreateDirectory(uploadPath);
                }

                foreach (var document in supportingDocuments)
                {
                    if (document != null && document.Length > 0)
                    {
                        // Generate a unique file name by prepending a GUID to the original file name, while keeping the original file name intact
                        var fileExtension = Path.GetExtension(document.FileName); // Get file extension
                        var originalFileName = Path.GetFileNameWithoutExtension(document.FileName); // Get original file name without extension
                        var fileName = $"{Guid.NewGuid()}_{originalFileName}{fileExtension}"; // Mix GUID with original file name
                        var fullPath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await document.CopyToAsync(stream);
                        }

                        var supportingDocument = new SupportingDocument
                        {
                            DocumentName = fileName, // Store the new GUID-prepended file name
                            FilePath = fullPath,
                            UserId = user.Id,
                            UploadedDate = DateTime.UtcNow
                        };

                        await _documentSupportRepository.CreateAsync(supportingDocument);
                        newApplicationJob.SupportingDocumentsIdNavigation.Add(supportingDocument);
                    }
                }
            }

            // Save the ApplicationJob entity
            await _applicationJobRepository.CreateAsync(newApplicationJob);

            // Record the workflow action
            var newWorkflowAction = new WorkflowAction
            {
                ProcessId = newProcess.ProcessId,
                StepId = nextStep.CurrentStepId,
                ActorId = user.Id,
                Action = "Submitted",
                ActionDate = DateTime.UtcNow,
                Comments = "Submitted job application"
            };
            await _workflowActionRepository.CreateAsync(newWorkflowAction);

            var actorEmails = new List<string>();
            // Get requester email
            var requesterEmail = user.Email!;
            actorEmails.Add(requesterEmail);

            // Check if there is any next actor available
            // If exist, add actor email to actorEmails
            var updatedProcess = await _processRepository.GetByIdAsync(newProcess.ProcessId);

            if (updatedProcess!.WorkflowSequence.RequiredRole != null)
            {
                var nextActorRoleName = newProcess.WorkflowSequence.Role.Name;

                // Retrieve all recruiters in the same company as the job post
                var recruiters = await _userManager.GetUsersInRoleAsync("Recruiter");
                var recruitersInCompany = recruiters.Where(r => r.CompanyId == jobPost.CompanyId).Select(r => r.Email).ToList();

                actorEmails.AddRange(recruitersInCompany);

                var htmlTemplate = System.IO.File.ReadAllText(@"./Templates/EmailTemplates/SubmitApplicationJob.html");
                htmlTemplate = htmlTemplate.Replace("{{Name}}", $"{user.FirstName} {user.LastName}");
                htmlTemplate = htmlTemplate.Replace("{{Email}}", $"{user.Email}");
                htmlTemplate = htmlTemplate.Replace("{{PhoneNumber}}", $"{user.PhoneNumber}");
                htmlTemplate = htmlTemplate.Replace("{{Address}}", $"{user.Address}");
                htmlTemplate = htmlTemplate.Replace("{{WorkExperience}}", request.WorkExperience);
                htmlTemplate = htmlTemplate.Replace("{{Education}}", request.Education);
                htmlTemplate = htmlTemplate.Replace("{{Skills}}", request.Skills);
                htmlTemplate = htmlTemplate.Replace("{{UploadedDate}}", newApplicationJob.UploadedDate.ToString());
                htmlTemplate = htmlTemplate.Replace("{{JobPostId}}", request.JobPostId.ToString());

                // Prepare email attachments for supporting documents
                var emailAttachments = new List<AttachmentDto>();

                foreach (var doc in newApplicationJob.SupportingDocumentsIdNavigation)
                {
                    if (System.IO.File.Exists(doc.FilePath))
                    {
                        var fileContent = await System.IO.File.ReadAllBytesAsync(doc.FilePath);
                        var fileMimeType = MimeMapping.MimeUtility.GetMimeMapping(doc.FilePath);
                        emailAttachments.Add(new AttachmentDto
                        {
                            Content = fileContent,
                            FileName = doc.DocumentName,
                            MimeType = fileMimeType
                        });
                    }
                }

                var mailData = new EmailDataDto
                {
                    EmailSubject = "Application submitted to Recruiter Review",
                    EmailBody = htmlTemplate,
                    EmailToIds = actorEmails,
                    AttachmentFiles = emailAttachments
                };

                var emailResult = _emailService.SendEmailAsync(mailData);
            }

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Application job submitted successfully!"
            };
        }

        public async Task<IEnumerable<object>> GetAllApplicationStatuses()
        {
            // Get user and roles from HttpContextAccessor
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);
            var userRoles = await _userManager.GetRolesAsync(user!);

            var applications = new List<ApplicationJob>();

            foreach (var role in userRoles)
            {
                if (role == "Applicant")
                {
                    // Fetch all applications where the user is the applicant
                    var applicantApplications = await _applicationJobRepository.GetAllByApplicantAsync(r => r.UserId == user!.Id);
                    applications.AddRange(applicantApplications);
                }
                else
                {
                    // Ensure CompanyId has a value for non-applicant roles
                    if (!user.CompanyId.HasValue)
                    {
                        throw new InvalidOperationException("User does not have a company associated.");
                    }

                    // Fetch applications linked to the user's company and role
                    var roleApplications = await _applicationJobRepository.GetAllToStatusAsync(user.CompanyId.Value, role);
                    applications.AddRange(roleApplications);
                }
            }

            // Project the results into a more user-friendly format
            var applicationStatuses = applications.Select(app => new
            {
                ApplicationId = app.ApplicationJobId,
                ApplicantName = $"{app.UserIdNavigation.FirstName} {app.UserIdNavigation.LastName}",
                JobTitle = app.JobPostNavigation.JobTitle,
                Status = app.ProcessIdNavigation.Status,
                Comments = app.ProcessIdNavigation.WorkflowActions.Select(wa => wa.Comments).LastOrDefault() // Get last comment or null if none
            }).ToList();

            return applicationStatuses;
        }

        public async Task<SupportingDocumentResponseDto> GetAllSupportingDocuments()
        {
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);

            var docs = await _documentSupportRepository.GetAllAsync();

            var userDocs = docs.Where(d => d.UserId == user!.Id).Select(x => new
            {
                DocumentId = x.SupportingDocumentId,
                DocumentName = x.DocumentName,
                UploadedDate = x.UploadedDate
            });

            return new SupportingDocumentResponseDto
            {
                Status = "Success",
                Message = "Documents retrieved successfuly",
                Documents = userDocs
            };
        }

        public async Task<SupportingDocumentResponseDto> GetSupportingDocumentById(int id)
        {
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);

            var doc = await _documentSupportRepository.GetByIdAsync(id);

            if (doc == null)
            {
                return new SupportingDocumentResponseDto
                {
                    Status = "Error",
                    Message = "Document does not exist"
                };
            }

            if (doc.UserId != user!.Id)
            {
                return new SupportingDocumentResponseDto
                {
                    Status = "Error",
                    Message = "Unauthorized to retrieve document"
                };
            }

            var userDoc = new
            {
                DocumentId = doc.SupportingDocumentId,
                DocumentName = doc.DocumentName,
                UploadedDate = doc.UploadedDate
            };

            return new SupportingDocumentResponseDto
            {
                Status = "Success",
                Message = "Document retrieved successfuly",
                Documents = [userDoc]
            };
        }

        public async Task<IEnumerable<object>> GetAllIncomingApplications()
        {
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);
            var userRoles = await _userManager.GetRolesAsync(user!);
            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}/uploads/";

            var applications = new List<ApplicationJob>();
            var applicantApplications = await _applicationJobRepository.GetAllByApplicantAsync(aj => userRoles.Contains(aj.ProcessIdNavigation.WorkflowSequence.Role.Name) && aj.JobPostNavigation.CompanyId == user.CompanyId);
            applications.AddRange(applicantApplications);

            // Fetch supporting documents in a separate query first
            var supportingDocuments = await _documentSupportRepository.GetAllAsync(d => applications.Select(a => a.UserId).Contains(d.UserId));

            var applicationStatuses = applications.Select(app => new
            {
                ApplicationId = app.ApplicationJobId,
                ApplicantName = $"{app.UserIdNavigation.FirstName} {app.UserIdNavigation.LastName}",
                ProcessId = app.ProcessId,
                JobTitle = app.JobPostNavigation.JobTitle,
                Status = app.ProcessIdNavigation.Status,
                WorkExperience = app.WorkExperience,
                Education = app.Education,
                Skills = app.Skills,
                Comments = app.ProcessIdNavigation.WorkflowActions.Select(wa => wa.Comments).LastOrDefault(),
                SupportingDocuments = supportingDocuments.Where(sd => sd.UserId == app.UserId)
                    .Select(sd => new
                    {
                        FileName = sd.DocumentName,
                        FileUrl = $"{baseUrl}{Uri.EscapeDataString(sd.DocumentName)}"
                    }).ToList()
            }).ToList();

            return applicationStatuses;
        }

        public async Task<BaseResponseDto> ReviewJobApplication(ReviewRequestDto reviewRequest)
        {
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);
            var userRoles = await _userManager.GetRolesAsync(user!);
            var userRole = userRoles.Single();

            // get process
            var process = await _processRepository.GetByIdAsync(reviewRequest.ProcessId);

            if (process == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Process does not exist"
                };
            }

            if (process.WorkflowSequence.RequiredRole == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Process already finished"
                };
            }

            // get job application company id from job post
            var jobApplicationCompanyId = process.ApplicationJobNavigation.Select(x => x.JobPostNavigation.CompanyId).Single();

            if (jobApplicationCompanyId != user!.CompanyId || process.WorkflowSequence.Role.Name != userRole)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Unauthorize to review request"
                };
            }

            var newWorkflowAction = new WorkflowAction
            {
                ProcessId = process.ProcessId,
                StepId = process.CurrentStepId,
                ActorId = user.Id,
                Action = reviewRequest.Action,
                ActionDate = DateTime.UtcNow,
                Comments = reviewRequest.Comment
            };

            await _workflowActionRepository.CreateAsync(newWorkflowAction);

            // get nextStepId
            var nextStepRule = await _nextStepRuleRepository.GetFirstOrDefaultAsync(nsr => nsr.CurrentStepId == process.CurrentStepId && nsr.ConditionValue == reviewRequest.Action);
            var nextStepId = nextStepRule!.NextStepId;

            // update process
            process.Status = $"{reviewRequest.Action} by {userRole}";
            process.CurrentStepId = nextStepId;
            await _processRepository.UpdateAsync(process);

            // send email logic

            
            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Job Application Reviewed Sucessfuly"
            };
        }
    }
}
