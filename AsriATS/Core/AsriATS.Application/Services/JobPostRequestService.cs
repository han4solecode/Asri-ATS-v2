using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.JobPostRequest;
using AsriATS.Application.DTOs;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;
using AsriATS.Application.DTOs.Register;

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
    }
}
