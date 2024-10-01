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
                SupportingDocumentsId = request.SupportingDocumentsId,
                SupportingDocumentsIdNavigation = new List<SupportingDocument>() // Initialize the list of supporting documents
            };

            // Add additional documents if provided
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
                        var fileName = Path.GetFileName(document.FileName);
                        var fullPath = Path.Combine(uploadPath, fileName);

                        using (var stream = new FileStream(fullPath, FileMode.Create))
                        {
                            await document.CopyToAsync(stream);
                        }

                        var supportingDocument = new SupportingDocument
                        {
                            DocumentName = fileName,
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
                    var applicantApplications = await _applicationJobRepository.GetAllByApplicantAsync(user.Id);
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
    }
}
