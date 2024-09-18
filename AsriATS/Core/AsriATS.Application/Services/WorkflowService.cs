using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Workflow;
using AsriATS.Application.DTOs;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using AsriATS.Application.DTOs.WorkflowSequence;
using AsriATS.Application.DTOs.NextStepRule;

namespace AsriATS.Application.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepository;
        private readonly INextStepRuleRepository _nextStepRuleRepository;
        private readonly IWorkflowSequenceRepository _workflowSequenceRepository;
        public WorkflowService (IWorkflowRepository workflowRepository, INextStepRuleRepository nextStepRuleRepository, IWorkflowSequenceRepository workflowSequenceRepository)
        {
            _workflowRepository = workflowRepository;
            _nextStepRuleRepository = nextStepRuleRepository;
            _workflowSequenceRepository = workflowSequenceRepository;
        }

        public async Task<BaseResponseDto> CreateWorkflowAsync(WorkflowRequestDto request)
        {
            var newWorkflow = new Workflow
            {
                WorkflowName = request.WorkflowName,
                Description = request.Description
            };
            await _workflowRepository.CreateAsync(newWorkflow);
            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Workflow created successfully"
            };
        }
        public async Task<BaseResponseDto> CreateWorkflowSequenceAsync(WorkflowSequenceRequestDto request)
        {
            var workflow = await _workflowRepository.GetByIdAsync(request.WorkflowId);
            if (workflow == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Workflow not found"
                };
            }
            var newWorkflowSequence = new WorkflowSequence
            {
                WorkflowId = request.WorkflowId,
                StepOrder = request.StepOrder,
                StepName = request.StepName,
                RequiredRole = request.RequiredRole
            };
            await _workflowSequenceRepository.CreateAsync(newWorkflowSequence);
            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Workflow Sequence created successfully"
            };
        }
        public async Task<BaseResponseDto> CreateNextStepRuleAsync(NextStepRuleRequestDto request)
        {
            var currentStep = await _workflowSequenceRepository.GetByIdAsync(request.CurrentStepId);
            var nextStep = await _workflowSequenceRepository.GetByIdAsync(request.NextStepId);
            if (currentStep == null || nextStep == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Step not found"
                };
            }
            var newNextStepRule = new NextStepRule
            {
                CurrentStepId = request.CurrentStepId,
                NextStepId = request.NextStepId,
                ConditionType = request.ConditionType,
                ConditionValue = request.ConditionValue,
            };
            await _nextStepRuleRepository.CreateAsync(newNextStepRule);
            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Next Step created successfully"
            };
        }
    }
}
