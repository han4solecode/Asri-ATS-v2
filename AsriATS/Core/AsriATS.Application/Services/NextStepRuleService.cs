using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using AsriATS.Application.DTOs.NextStepRule;

namespace AsriATS.Application.Services
{
    public class NextStepRuleService : INextStepRuleService
    {
        private readonly INextStepRuleRepository _nextStepRuleRepository;
        private readonly IWorkflowSequenceRepository _workflowSequenceRepository;
        public NextStepRuleService(INextStepRuleRepository nextStepRuleRepository, IWorkflowSequenceRepository workflowSequenceRepository)
        {
            _nextStepRuleRepository = nextStepRuleRepository;
            _workflowSequenceRepository = workflowSequenceRepository;
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
