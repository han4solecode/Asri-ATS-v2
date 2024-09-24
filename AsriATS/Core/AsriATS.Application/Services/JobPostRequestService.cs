﻿using AsriATS.Application.Contracts;
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

        public JobPostRequestService(ICompanyRepository companyRepository, IJobPostRequestRepository jobPostRequestRepository, INextStepRuleRepository nextStepRuleRepository,
            IWorkflowSequenceRepository workflowSequenceRepository, IProcessRepository processRepository, IWorkflowRepository workflowRepository, IWorkflowActionRepository
            workflowActionRepository, UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor)
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
        }

        public async Task<BaseResponseDto> SubmitJobPostRequest(JobPostRequestDto request)
        {
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);
            if (company == null)
            {
                return new RegisterResponseDto
                {
                    Status = "Error",
                    Message = "Company is not registered!"
                };
            }
            var userName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
            var user = await _userManager.FindByNameAsync(userName);

            var workflow = await _workflowRepository.GetFirstOrDefaultAsync(w => w.WorkflowName == "Job Post Request");
            var currentStepId = await _workflowSequenceRepository.GetFirstOrDefaultAsync(wfs => wfs.WorkflowId == workflow.WorkflowId);
            var nextStepId = await _nextStepRuleRepository.GetFirstOrDefaultAsync(n => n.CurrentStepId == currentStepId.StepId);

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

            var newJobPostRequest = new JobPostRequest
            {
                JobTitle = request.JobTitle,
                CompanyId = request.CompanyId,
                Description = request.Description,
                Requirements = request.Requirements,
                Location = request.Location,
                MinSalary = request.MinSalary,
                MaxSalary = request.MaxSalary,
                EmploymentType = request.EmploymentType,
                ProcessId = newProcess.ProcessId
            };

            await _jobPostRequestRepository.CreateAsync(newJobPostRequest);

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

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Submitted post job request success"
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

            if (process.Requester.CompanyId != user!.CompanyId)
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
            if (process.WorkflowSequence.RequiredRole != null)
            {
                var nextActorRoleName = process.WorkflowSequence.Role.Name;
                var users = await _userManager.GetUsersInRoleAsync(nextActorRoleName!);
                var nextActorEmails = users.Where(a => a.CompanyId == user.CompanyId).Select(a => a.Email).ToList();
                // append nextActorEmails to actorEmails
                actorEmails.AddRange(nextActorEmails);
            }

            // send email notification
            // TODO: email template

            var mail = new EmailDataDto
            {
                EmailToIds = [requesterEmail],
                EmailCCIds = actorEmails!,
                EmailSubject = "Job Post Request",
                EmailBody = "Email Body" // TODO
            };

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Request Reviewed Sucessfuly"
            };
        }
    }
}
