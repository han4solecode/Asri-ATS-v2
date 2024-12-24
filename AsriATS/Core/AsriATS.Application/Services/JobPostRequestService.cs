using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.JobPostRequest;
using AsriATS.Application.DTOs;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using AsriATS.Application.DTOs.Register;
using AsriATS.Application.DTOs.Request;
using AsriATS.Application.DTOs.Email;

namespace AsriATS.Application.Services
{
    public class JobPostRequestService : IJobPostRequestService
    {
        private readonly ICompanyRepository _companyRepository;
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

        public JobPostRequestService(ICompanyRepository companyRepository, IJobPostRequestRepository jobPostRequestRepository, INextStepRuleRepository nextStepRuleRepository,
            IWorkflowSequenceRepository workflowSequenceRepository, IProcessRepository processRepository, IWorkflowRepository workflowRepository, IWorkflowActionRepository
            workflowActionRepository, UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor, IJobPostRepository jobPostRepository, RoleManager<AppRole> roleManager,
            IEmailService emailService)
        {
            _companyRepository = companyRepository;
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
        }

        public async Task<BaseResponseDto> SubmitJobPostRequest(JobPostRequestDto request)
        {
            // Retrieve the company from the repository using the provided CompanyId

            //var company = await _companyRepository.GetByIdAsync(request.CompanyId);
            //if (company == null)
            //{
            //    return new RegisterResponseDto
            //    {
            //        Status = "Error",
            //        Message = "Company is not registered!"
            //    };
            //}

            // Get the current logged-in user
            var userName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

            // Get the user information from UserManager
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "User not found!"
                };
            }

            // Check if the user (recruiter) has a valid CompanyId and is part of the same company
            if (!user.CompanyId.HasValue)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "User is not associated with any company."
                };
            }

            var recruiterCompany = await _companyRepository.GetByIdAsync(user.CompanyId.Value);
            if (recruiterCompany == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "You are not authorized to submit job posts for this company."
                };
            }

            // Get the workflow for job post requests
            var workflow = await _workflowRepository.GetFirstOrDefaultAsync(w => w.WorkflowName == "Job Post Request");
            if (workflow == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Workflow for job post request not found!"
                };
            }

            // Get the current step and next step in the workflow
            var currentStepId = await _workflowSequenceRepository.GetFirstOrDefaultAsync(wfs => wfs.WorkflowId == workflow.WorkflowId);
            if (currentStepId == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Current workflow step not found!"
                };
            }

            var nextStepId = await _nextStepRuleRepository.GetFirstOrDefaultAsync(n => n.CurrentStepId == currentStepId.StepId);
            if (nextStepId == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Next step in the workflow not found!"
                };
            }

            // Create a new process for the job post request
            var newProcess = new Process
            {
                RequesterId = user.Id,
                WorkflowId = workflow.WorkflowId,
                RequestType = "Job Post",
                Status = "Pending Approval",
                RequestDate = DateTime.UtcNow,
                CurrentStepId = nextStepId.NextStepId,
            };

            await _processRepository.CreateAsync(newProcess);

            // Create a new job post request
            var newJobPostRequest = new JobPostRequest
            {
                JobTitle = request.JobTitle,
                CompanyId = user?.CompanyId ?? 0,
                Description = request.Description,
                Requirements = request.Requirements,
                Location = request.Location,
                MinSalary = request.MinSalary,
                MaxSalary = request.MaxSalary,
                EmploymentType = request.EmploymentType,
                ProcessId = newProcess.ProcessId
            };

            await _jobPostRequestRepository.CreateAsync(newJobPostRequest);

            // Record the workflow action
            var newWorkflowAction = new WorkflowAction
            {
                ProcessId = newProcess.ProcessId,
                StepId = nextStepId.CurrentStepId,
                ActorId = user.Id,
                Action = "Submit",
                ActionDate = DateTime.UtcNow,
                Comments = request.Comments
            };

            await _workflowActionRepository.CreateAsync(newWorkflowAction);

            var actorEmails = new List<string>();
            // get requester email
            var requesterEmail = user.Email!;
            actorEmails.Add(requesterEmail);

            // check if there is any next actor available
            // if exist, add actor email to actorEmails
            var updatedProcess = await _processRepository.GetByIdAsync(newProcess.ProcessId);
            if (updatedProcess!.WorkflowSequence.RequiredRole != null)
            {
                var nextActorRoleName = newProcess.WorkflowSequence.Role.Name;
                var users = await _userManager.GetUsersInRoleAsync(nextActorRoleName!);
                var nextActorEmails = users.Where(a => a.CompanyId == user.CompanyId).Select(a => new { a.Email, a.FirstName }).ToList();
                // append nextActorEmails to actorEmails
                actorEmails.AddRange(nextActorEmails.Select(a => a.Email));

                var htmlTemplate = System.IO.File.ReadAllText(@"./Templates/EmailTemplates/RecruiterJobPostRequestToHR.html");
                htmlTemplate = htmlTemplate.Replace("{{JobTitle}}", request.JobTitle);
                htmlTemplate = htmlTemplate.Replace("{{Description}}", request.Description);
                htmlTemplate = htmlTemplate.Replace("{{Requirements}}", request.Requirements);
                htmlTemplate = htmlTemplate.Replace("{{Location}}", request.Location);
                htmlTemplate = htmlTemplate.Replace("{{MinSallary}}", request.MinSalary.ToString());
                htmlTemplate = htmlTemplate.Replace("{{MaxSallary}}", request.MaxSalary.ToString());
                htmlTemplate = htmlTemplate.Replace("{{EmploymentType}}", request.EmploymentType);
                htmlTemplate = htmlTemplate.Replace("{{RecruiterName}}", $"{user.FirstName} {user.LastName}");

                foreach (var name in nextActorEmails)
                {
                    htmlTemplate = htmlTemplate.Replace("{{Name}}", $"{name.FirstName}");
                }

                var mailData = new EmailDataDto
                {
                    EmailSubject = "Post job request submitted to HR Review",
                    EmailBody = htmlTemplate,
                    EmailToIds = actorEmails
                };

                var emailResult = _emailService.SendEmailAsync(mailData);
            }

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Submitted job post request successfully"
            };
        }

        public async Task<BaseResponseDto> ReviewJobPostRequest(ReviewRequestDto reviewRequest)
        {
            // get user and role from httpcontextaccessor
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);
            var userRoles = await _userManager.GetRolesAsync(user!);
            var userRole = userRoles.Single();

            var process = await _processRepository.GetByIdAsync(reviewRequest.ProcessId);

            if (process == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Process does not exist"
                };
            }

            // check if process has a requiredRole, if null then return error
            if (process.WorkflowSequence.RequiredRole == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Process already finished"
                };
            }

            if (process.Requester.CompanyId != user!.CompanyId || process.WorkflowSequence.Role.Name != userRole)
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

            var requests = await _jobPostRequestRepository.GetAllAsync();
            var reqData = requests.Where(r => r.ProcessId == process.ProcessId).Single();

            // if approved, create new JobPost
            if (reviewRequest.Action == "Approved")
            {
                var newJobPost = new JobPost
                {
                    JobTitle = reqData.JobTitle,
                    CompanyId = reqData.CompanyId,
                    Description = reqData.Description,
                    Requirements = reqData.Requirements,
                    Location = reqData.Location,
                    MinSalary = reqData.MinSalary,
                    MaxSalary = reqData.MaxSalary,
                    EmploymentType = reqData.EmploymentType,
                    CreatedDate =DateTime.UtcNow
                };

                await _jobPostRepository.CreateAsync(newJobPost);
            }

            // send email to other actors
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
            var emailTemplate = File.ReadAllText(@"./Templates/EmailTemplates/ReviewJobPostRequest.html");

            emailTemplate = emailTemplate.Replace("{{Name}}", $"{process.Requester.FirstName} {process.Requester.LastName}");
            emailTemplate = emailTemplate.Replace("{{UserId}}", process.RequesterId);
            emailTemplate = emailTemplate.Replace("{{Email}}", requesterEmail);
            emailTemplate = emailTemplate.Replace("{{JobTitle}}", reqData.JobTitle);
            emailTemplate = emailTemplate.Replace("{{Description}}", reqData.Description);
            emailTemplate = emailTemplate.Replace("{{Requirements}}", reqData.Requirements);
            emailTemplate = emailTemplate.Replace("{{Location}}", reqData.Location);
            emailTemplate = emailTemplate.Replace("{{SalaryRange}}", $"{reqData.MinSalary} - {reqData.MaxSalary}");
            emailTemplate = emailTemplate.Replace("{{EmploymentType}}", reqData.EmploymentType);
            emailTemplate = emailTemplate.Replace("{{Status}}", process.Status);
            emailTemplate = emailTemplate.Replace("{{Comments}}", process.WorkflowActions.Select(wa => wa.Comments).Last());

            var mail = new EmailDataDto
            {
                EmailToIds = [requesterEmail],
                EmailCCIds = actorEmails!,
                EmailSubject = "Job Post Request",
                EmailBody = emailTemplate
            };

            await _emailService.SendEmailAsync(mail);

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Request Reviewed Sucessfuly"
            };
        }

        public async Task<IEnumerable<object>> GetJobPostRequestToReview()
        {
            // get user and role from httpcontextaccessor
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);
            var userRoles = await _userManager.GetRolesAsync(user!);
            var userRole = userRoles.Single();

            var role = await _roleManager.FindByNameAsync(userRole);
            var roleId = role!.Id;

            // retrieve job post requests
            var requests = await _jobPostRequestRepository.GetAllToBeReviewedAsync(user!.CompanyId!.Value, roleId);

            var a = requests.Select(r => new
            {
                ProcessId = r.ProcessId,
                Requester = $"{r.ProcessIdNavigation.Requester.FirstName} {r.ProcessIdNavigation.Requester.LastName}",
                RequestDate = r.ProcessIdNavigation.RequestDate,
                JobTitle = r.JobTitle,
                Description = r.Description,
                Requirements = r.Requirements,
                Location = r.Location,
                MinSalary = r.MinSalary,
                MaxSalary = r.MaxSalary,
                EmploymentType = r.EmploymentType,
                Status = r.ProcessIdNavigation.Status,
                Comments = r.ProcessIdNavigation.WorkflowActions.Select(wa => wa.Comments).Last()
            }).ToList();

            return a;
        }

        // Method for updated the request after need modification approval
        public async Task<BaseResponseDto> UpdateJobPostRequest(UpdateJobPostRequestDto requestDto)
        {
            // Get user from HttpContextAccessor
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);
            var userRoles = await _userManager.GetRolesAsync(user!);
            var userRole = userRoles.Single();

            var process = await _processRepository.GetByIdAsync(requestDto.ProcessId);

            if (process == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Process does not exist"
                };
            }

            // Check if the process requires modification
            if (process.WorkflowSequence.RequiredRole == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Process already finished"
                };
            }

            // Check if the user is authorized to review requests from their company
            if (process.Requester.CompanyId != user!.CompanyId)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Unauthorized to review request"
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
                    Message = $"Modification not allowed for the current process status. Required role: {requiredRole.Name}"
                };
            }

            // Retrieve the existing job post request using the ProcessId
            var jobPostRequest = await _jobPostRequestRepository.GetFirstOrDefaultAsync(jpr => jpr.ProcessId == process.ProcessId);

            if (jobPostRequest == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Job post request does not exist"
                };
            }

            // Update the job post request with the new data
            jobPostRequest.JobTitle = requestDto.JobTitle; // Update fields as necessary
            jobPostRequest.Description = requestDto.Description;
            jobPostRequest.Requirements = requestDto.Requirements;
            jobPostRequest.Location = requestDto.Location;
            jobPostRequest.MinSalary = requestDto.MinSalary;
            jobPostRequest.MaxSalary = requestDto.MaxSalary;
            jobPostRequest.EmploymentType = requestDto.EmploymentType;

            // Save the updated job post request
            await _jobPostRequestRepository.UpdateAsync(jobPostRequest);

            // Create a new workflow action for the update
            var newWorkflowAction = new WorkflowAction
            {
                ProcessId = process.ProcessId,
                StepId = process.CurrentStepId,
                ActorId = user.Id,
                Action = "Update",
                ActionDate = DateTime.UtcNow,
                Comments = requestDto.Comments // Assuming you want to capture comments
            };

            await _workflowActionRepository.CreateAsync(newWorkflowAction);

            // Send the process back for review by HR or next step
            // Get the next step ID
            // get nextStepId
            var nextStepRule = await _nextStepRuleRepository.GetFirstOrDefaultAsync(nsr => nsr.CurrentStepId == process.CurrentStepId && nsr.ConditionValue == newWorkflowAction.Action);
            var nextStepId = nextStepRule!.NextStepId;

            // update process
            process.Status = $"{newWorkflowAction.Action} by {userRole}";
            process.CurrentStepId = nextStepId;
            await _processRepository.UpdateAsync(process);

            var workflowActions = await _workflowActionRepository.GetAllAsync();
            var actorEmails = workflowActions
                        .Where(a => a.ProcessId == process.ProcessId && a.Actor != null && !string.IsNullOrEmpty(a.Actor.Email))
                        .Select(x => x.Actor.Email)
                        .Distinct()
                        .ToList();

            var requesterEmail = process.Requester.Email;
            actorEmails.Remove(requesterEmail);

            var updatedProcess = await _processRepository.GetByIdAsync(process.ProcessId);
            if (updatedProcess!.WorkflowSequence?.RequiredRole != null)
            {
                var nextActorRoleName = process.WorkflowSequence.Role.Name;
                var users = await _userManager.GetUsersInRoleAsync(nextActorRoleName!);
                var nextActorEmails = users.Where(a => a.CompanyId == user.CompanyId).Select(a => a.Email).ToList();
                actorEmails.AddRange(nextActorEmails);

                var nextActors = users
                        .Where(u => nextActorEmails.Contains(u.Email))
                        .Select(u => new { u.Email, u.FirstName })
                        .ToList();

                var htmlTemplate = System.IO.File.ReadAllText(@"./Templates/EmailTemplates/UpdateJobPostRequest.html");
                htmlTemplate = htmlTemplate.Replace("{{JobTitle}}", requestDto.JobTitle);
                htmlTemplate = htmlTemplate.Replace("{{Description}}", requestDto.Description);
                htmlTemplate = htmlTemplate.Replace("{{Requirements}}", requestDto.Requirements);
                htmlTemplate = htmlTemplate.Replace("{{Location}}", requestDto.Location);
                htmlTemplate = htmlTemplate.Replace("{{MinSallary}}", requestDto.MinSalary.ToString());
                htmlTemplate = htmlTemplate.Replace("{{MaxSallary}}", requestDto.MaxSalary.ToString());
                htmlTemplate = htmlTemplate.Replace("{{EmploymentType}}", requestDto.EmploymentType);
                htmlTemplate = htmlTemplate.Replace("{{RecruiterName}}", $"{user.FirstName} {user.LastName}");

                foreach (var nextActor in nextActors)
                {
                    var personalizedHtmlTemplate = htmlTemplate.Replace("{{Name}}", nextActor.FirstName);

                    var mailData = new EmailDataDto
                    {
                        EmailSubject = "Post job request submitted to HR Review",
                        EmailBody = personalizedHtmlTemplate,
                        EmailToIds = actorEmails  // Send to each individual actor
                    };

                    var emailResult = _emailService.SendEmailAsync(mailData);
                }
            }

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Job post request updated successfully and sent for review."
            };
        }

        public async Task<JobPostRequestResponseDto> GetJobPostRequest(int id)
        {
            var jobPostRequest = await _jobPostRequestRepository.GetFirstOrDefaultAsync(jpr => jpr.ProcessId == id);
            if (jobPostRequest == null)
            {
                return new JobPostRequestResponseDto
                {
                    Status = "Error",
                    Message = "Job post request is not found"
                };
            }
            var jobPostRequestDto = new JobPostRequestResponseDto
            {
                Requester = $"{jobPostRequest.ProcessIdNavigation.Requester.FirstName} {jobPostRequest.ProcessIdNavigation.Requester.LastName}",
                JobTitle = jobPostRequest.JobTitle,
                CompanyId = jobPostRequest.CompanyId,
                Description = jobPostRequest.Description,
                Requirements = jobPostRequest.Requirements,
                Location = jobPostRequest.Location,
                MinSalary = jobPostRequest.MinSalary,
                MaxSalary = jobPostRequest.MaxSalary,
                EmploymentType = jobPostRequest.EmploymentType,
                Status = "Success",
                Message = "Job post request get successfuly"
            };
            return jobPostRequestDto;
        }

        public async Task<IEnumerable<object>> GetJobPostRequestForRecruiter()
        {
            // get user and role from httpcontextaccessor
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);

            // retrieve job post requests
            var requests = await _jobPostRequestRepository.GetAllJobPostRequestsForRecruiter(user!.Id!);

            var a = requests.Select(r => new
            {
                ProcessId = r.ProcessId,
                Requester = $"{r.ProcessIdNavigation.Requester.FirstName} {r.ProcessIdNavigation.Requester.LastName}",
                RequestDate = r.ProcessIdNavigation.RequestDate,
                JobTitle = r.JobTitle,
                Description = r.Description,
                Requirements = r.Requirements,
                Location = r.Location,
                MinSalary = r.MinSalary,
                MaxSalary = r.MaxSalary,
                EmploymentType = r.EmploymentType,
                Status = r.ProcessIdNavigation.Status,
                Comments = r.ProcessIdNavigation.WorkflowActions.Select(wa => wa.Comments).Last()
            }).ToList();

            return a;
        }
    }
}
