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
using AsriATS.Application.DTOs.WorkflowAction;
using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.InterivewScheduling;

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
        private readonly IInterviewSchedulingRepository _interviewSchedulingRepository;

        public ApplicationJobService(IJobPostRequestRepository jobPostRequestRepository, INextStepRuleRepository nextStepRuleRepository, IWorkflowSequenceRepository workflowSequenceRepository, IWorkflowRepository workflowRepository, IProcessRepository processRepository, IWorkflowActionRepository workflowActionRepository, UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor, IJobPostRepository jobPostRepository, RoleManager<AppRole> roleManager, IEmailService emailService, IApplicationJobRepository applicationJobRepository, IDocumentSupportRepository documentSupportRepository, IInterviewSchedulingRepository interviewSchedulingRepository)
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
            _interviewSchedulingRepository = interviewSchedulingRepository;
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

                // Create a user-specific folder inside "uploads" based on the UserId
                var userUploadPath = Path.Combine(uploadPath, user.Id.ToString()); // Use UserId as folder name
                if (!Directory.Exists(userUploadPath))
                {
                    Directory.CreateDirectory(userUploadPath); // Create directory for the user if it doesn't exist
                }

                foreach (var document in supportingDocuments)
                {
                    if (document != null && document.Length > 0)
                    {
                        // Generate a unique file name by prepending a GUID to the original file name, while keeping the original file name intact
                        var fileExtension = Path.GetExtension(document.FileName); // Get file extension
                        var originalFileName = Path.GetFileNameWithoutExtension(document.FileName); // Get original file name without extension
                        var fileName = $"{Guid.NewGuid()}_{originalFileName}{fileExtension}"; // Mix GUID with original file name
                        var fullPath = Path.Combine(userUploadPath, fileName);

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

        public async Task<object> GetAllApplicationStatuses(Pagination pagination)
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
                    if (!user!.CompanyId.HasValue)
                    {
                        throw new InvalidOperationException("User does not have a company associated.");
                    }

                    // Fetch applications linked to the user's company and role
                    var roleApplications = await _applicationJobRepository.GetAllToStatusAsync(user.CompanyId.Value, role);
                    applications.AddRange(roleApplications);
                }
            }

            // Project the results into a user-friendly format
            var applicationStatuses = applications.Select(app => new
            {
                ApplicationId = app.ApplicationJobId,
                ApplicantName = $"{app.UserIdNavigation.FirstName} {app.UserIdNavigation.LastName}",
                JobTitle = app.JobPostNavigation.JobTitle,
                Status = app.ProcessIdNavigation.Status,
                ProcessId = app.ProcessIdNavigation.ProcessId,
                CurrentStep = app.ProcessIdNavigation.WorkflowSequence.StepName,
                Comments = app.ProcessIdNavigation.WorkflowActions.Select(wa => wa.Comments).LastOrDefault() // Get last comment or null if none
            });

            // Apply pagination
            var totalRecords = applicationStatuses.Count();
            var pageNumber = pagination.PageNumber ?? 1;
            var pageSize = pagination.PageSize ?? 20;
            var totalPages = (int)Math.Ceiling((double)totalRecords / pageSize);

            var paginatedData = applicationStatuses
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            return new
            {
                TotalRecords = totalRecords,
                TotalPages = totalPages,
                CurrentPage = pageNumber,
                PageSize = pageSize,
                Data = paginatedData
            };
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
                SupportingDocuments = app.SupportingDocumentsIdNavigation.Select(sd => new
                {
                    FileName = sd.DocumentName,
                    FileUrl = $"{baseUrl}{app.UserId}/" + Uri.EscapeDataString(sd.DocumentName)
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

            // next step rule validation
            var nextStepRule = await _nextStepRuleRepository.GetFirstOrDefaultAsync(nsr => nsr.CurrentStepId == process.CurrentStepId && nsr.ConditionValue == reviewRequest.Action);

            if (nextStepRule == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Action is not valid"
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
            var nextStepId = nextStepRule.NextStepId;

            // update process
            process.Status = $"{reviewRequest.Action} by {userRole}";
            process.CurrentStepId = nextStepId;
            await _processRepository.UpdateAsync(process);

            // send email logic
            // get other actors email
            var workflowActions = await _workflowActionRepository.GetAllAsync();
            var actorEmails = workflowActions.Where(a => a.ProcessId == process.ProcessId).Select(x => x.Actor.Email).Distinct().ToList();
            // get requester email
            var requesterEmail = process.Requester.Email;
            // remove requesterEmail from actorEmails so requester only receive the email once
            actorEmails.Remove(requesterEmail);

            // check if there is any next actor available
            // if exist, add actor email to actorEmails
            var updatedProcess = await _processRepository.GetByIdAsync(process.ProcessId);
            if (updatedProcess!.WorkflowSequence.RequiredRole != null)
            {
                var nextActorRoleName = process.WorkflowSequence.Role.Name;
                var users = await _userManager.GetUsersInRoleAsync(nextActorRoleName!);
                var nextActorEmails = users.Where(a => a.CompanyId == user.CompanyId).Select(a => a.Email).ToList();
                // append nextActorEmails to actorEmails
                actorEmails.AddRange(nextActorEmails);
            }

            // send email notification
            var emailTemplate = File.ReadAllText(@"./Templates/EmailTemplates/ReviewJobApplication.html");

            emailTemplate = emailTemplate.Replace("{{Name}}", $"{process.Requester.FirstName} {process.Requester.LastName}");
            emailTemplate = emailTemplate.Replace("{{JobTitle}}", $"{process.ApplicationJobNavigation.Select(x => x.JobPostNavigation.JobTitle).Single()}");
            emailTemplate = emailTemplate.Replace("{{CompanyName}}", $"{process.ApplicationJobNavigation.Select(x => x.JobPostNavigation.CompanyIdNavigation.Name).Single()}");
            emailTemplate = emailTemplate.Replace("{{Status}}", $"{process.Status}");
            emailTemplate = emailTemplate.Replace("{{Comment}}", $"{process.WorkflowActions.Select(wa => wa.Comments).LastOrDefault()}");

            var mail = new EmailDataDto
            {
                EmailToIds = [requesterEmail],
                EmailCCIds = actorEmails!,
                EmailSubject = "Job Application Update",
                EmailBody = emailTemplate
            };

            await _emailService.SendEmailAsync(mail);
            
            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Job Application Reviewed Sucessfuly"
            };
        }

        public async Task<BaseResponseDto> UpdateApplicationJob(UpdateApplicationJobDto requestDto, List<IFormFile>? supportingDocuments = null)
        {
            // Get the user from HttpContextAccessor
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);
            var userRoles = await _userManager.GetRolesAsync(user!);
            var userRole = userRoles.Single();

            if (user == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "User not found"
                };
            }

            // Retrieve the process linked to the application
            var process = await _processRepository.GetByIdAsync(requestDto.ProcessId);

            if (process.WorkflowSequence.RequiredRole == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Process not found or does not require request additional information"
                };
            }

            // Retrieve the required role as an ID
            var requiredRoleId = process.WorkflowSequence.RequiredRole;

            // Retrieve the actual role name from the database (using RoleManager or any custom role repository)
            var requiredRole = await _roleManager.FindByIdAsync(requiredRoleId);

            // Compare the user's role with the required role name
            if (!requiredRole.Name.Equals(userRole, StringComparison.OrdinalIgnoreCase))
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = $"Request additional information not allowed for the current process status. Required role: {requiredRole.Name}"
                };
            }

            // Ensure the application belongs to the current user
            var application = await _applicationJobRepository.GetFirstOrDefaultAsyncUpdate(aj => aj.ProcessId == requestDto.ProcessId && aj.UserId == user.Id, include: query => query.Include(aj => aj.SupportingDocumentsIdNavigation));

            if (application == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",

                    Message = "Application job not found"
                };
            }

            // Update the application job details (e.g., skills, work experience, education)
            application.WorkExperience = requestDto.WorkExperience;
            application.Education = requestDto.Education;
            application.Skills = requestDto.Skills;
            application.UploadedDate = DateTime.UtcNow;

            // Handle supporting document updates
            if (requestDto.SupportingDocuments != null && requestDto.SupportingDocuments.Any())
            {
                foreach (var documentDto in requestDto.SupportingDocuments)
                {
                    var existingDocument = await _documentSupportRepository.GetByIdAsync(documentDto.SupportingDocumentsId);

                    if (existingDocument != null && existingDocument.UserId == user.Id)
                    {
                        // Update document if it already exists
                        existingDocument.DocumentName = documentDto.DocumentName;
                        existingDocument.FilePath = documentDto.FilePath; // Assuming URL is updated here
                        await _documentSupportRepository.UpdateAsync(existingDocument);
                    }
                    else
                    {
                        // Add new document if not found
                        var newDocument = new SupportingDocument
                        {
                            UserId = user.Id,
                            DocumentName = documentDto.DocumentName,
                            FilePath = documentDto.FilePath
                        };
                        await _documentSupportRepository.CreateAsync(newDocument);
                    }
                }
            }

            // Handle additional new documents if provided
            if (supportingDocuments != null && supportingDocuments.Any())
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
                        // Generate unique file name
                        var fileExtension = Path.GetExtension(document.FileName);
                        var originalFileName = Path.GetFileNameWithoutExtension(document.FileName);
                        var fileName = $"{Guid.NewGuid()}_{originalFileName}{fileExtension}";
                        var fullPath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await document.CopyToAsync(stream);
                        }

                        var newSupportingDocument = new SupportingDocument
                        {
                            DocumentName = fileName,
                            FilePath = fullPath,
                            UserId = user.Id,
                            UploadedDate = DateTime.UtcNow
                        };

                        // Ensure SupportingDocumentsIdNavigation is initialized
                        if (application.SupportingDocumentsIdNavigation == null)
                        {
                            application.SupportingDocumentsIdNavigation = new List<SupportingDocument>();
                        }

                        // Save the new supporting document to the database
                        await _documentSupportRepository.CreateAsync(newSupportingDocument);

                        // Add the new document to the application's navigation property
                        application.SupportingDocumentsIdNavigation.Add(newSupportingDocument);
                    }
                }
            }

            // Save updated application job
            await _applicationJobRepository.UpdateAsync(application);

            // Log the modification in the workflow
            var workflowAction = new WorkflowAction
            {
                ProcessId = process.ProcessId,
                StepId = process.CurrentStepId,
                ActorId = user.Id,
                Action = "Update",
                ActionDate = DateTime.UtcNow,
                Comments = requestDto.Comments
            };
            await _workflowActionRepository.CreateAsync(workflowAction);

            // Move to the next step if applicable
            var nextStepRule = await _nextStepRuleRepository.GetFirstOrDefaultAsync(nsr => nsr.CurrentStepId == process.CurrentStepId && nsr.ConditionValue == workflowAction.Action);
            var nextStepId = nextStepRule!.NextStepId;

            // update process
            process.Status = $"{workflowAction.Action} by {userRole}";
            process.CurrentStepId = nextStepId;
            await _processRepository.UpdateAsync(process);

            // Send notification to relevant actors (HR or Recruiters) for further review
            // Prepare email notification content
            var actorEmails = new List<string> { user.Email! };

            // Get additional actor emails (e.g., recruiters, etc.)
            var recruiters = await _userManager.GetUsersInRoleAsync("Recruiter");
            var recruitersInCompany = recruiters.Where(r => r.CompanyId == application.JobPostNavigation.CompanyId).Select(r => r.Email).ToList();
            actorEmails.AddRange(recruitersInCompany);

            var htmlTemplate = System.IO.File.ReadAllText(@"./Templates/EmailTemplates/UpdateApplicationJob.html");
            htmlTemplate = htmlTemplate.Replace("{{Name}}", $"{user.FirstName} {user.LastName}");
            htmlTemplate = htmlTemplate.Replace("{{WorkExperience}}", application.WorkExperience);
            htmlTemplate = htmlTemplate.Replace("{{Education}}", application.Education);
            htmlTemplate = htmlTemplate.Replace("{{Skills}}", application.Skills);
            htmlTemplate = htmlTemplate.Replace("{{UploadedDate}}", application.UploadedDate.ToString());

            // Attach updated documents
            var emailAttachments = new List<AttachmentDto>();
            if (application.SupportingDocumentsIdNavigation != null)
            {
                foreach (var doc in application.SupportingDocumentsIdNavigation)
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
            }

            // Prepare the email data
            var mailData = new EmailDataDto
            {
                EmailSubject = "Application Job Update Notification",
                EmailBody = htmlTemplate,
                EmailToIds = actorEmails,
                AttachmentFiles = emailAttachments
            };

            // Send the email notification
            await _emailService.SendEmailAsync(mailData);

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Application job updated successfully. Reviewers have been notified."
            };
        }

        public async Task<ApplicationDetailDto> GetProcessAsync(int processId)
        {
            var process = await _processRepository.GetByIdAsync(processId);
            if (process == null)
            {
                throw new NullReferenceException("Process not found.");
            }

            var app = await _processRepository.GetByProcessIdAsync(processId);
            if (app == null)
            {
                throw new NullReferenceException("Leave request not found for the given process ID.");
            }

            var request = _httpContextAccessor.HttpContext.Request;
            var baseUrl = $"{request.Scheme}://{request.Host}/uploads/";

            var workflowActions = await _workflowActionRepository.GetByProcessIdAsync(process.ProcessId) ?? new List<WorkflowAction>();
            var supportingDoc = await _documentSupportRepository.GetByApplicationJobIdAsync(app.ApplicationJobId);
            var interview = await _interviewSchedulingRepository.GetByProcessIdAsync(process.ProcessId);

            var processDetailDto = new ApplicationDetailDto
            {
                ApplicationJobId = app.ApplicationJobId,
                ProcessId = process.ProcessId,
                Name = app.Name ?? "No name provided",
                Email = app.Email ?? "No email provided",
                PhoneNumber = app.PhoneNumber ?? "Unknown",
                Address = app.Address ?? "No address available",
                WorkExperience = app.WorkExperience ?? "No Work Experience provided",
                Education = app.Education ?? "No Education provided",
                Skills = app.Skills ?? "No skills provided",
                Status = process.Status ?? "No status available",
                CurrentStep = process.WorkflowSequence?.StepName ?? "No step available",
                RequiredRole = process.WorkflowSequence?.Role?.Name ?? "No role available",
                JobPostName = app.JobPostNavigation.JobTitle ?? "No Job Post Name",
                UploadedDate = app.UploadedDate,
                SupportingDocuments = supportingDoc.Select(sd => new SupportingDocumentDto
                {
                    SupportingDocumentsId = sd.SupportingDocumentId,
                    DocumentName = sd.DocumentName,
                    FilePath = $"{baseUrl}{app.UserId}/" + Uri.EscapeDataString(sd.DocumentName),
                    UploadedDate = sd.UploadedDate,
                }).ToList(),
                WorkflowActions = workflowActions.Select(action => new WorkflowActionDto
                {
                    ActionDate = action.ActionDate,
                    ActionBy = action.Actor?.UserName ?? "Unknown",
                    Action = action.Action ?? "No action",
                    Comments = action.Comments ?? "No comments"
                }).ToList(),
                InterviewSchedulingDetails = interview.Select(interviews => new InterviewSchedulingDetailsDto
                {
                    InterviewSchedulingId = interviews.InterviewSchedulingId,
                    ApplicationId = interviews.ApplicationId,
                    ProcessId = interviews.ProcessId,
                    InterviewTime = interviews.InterviewTime,
                    InterviewType = interviews.InterviewType,
                    Interviewer = interviews.Interviewer,
                    InterviewersComments = interviews.InterviewersComments,
                    IsConfirmed = interviews.IsConfirmed,
                    Location = interviews.Location,
                }).ToList(),
            };

            return processDetailDto;
        }

    }
}
