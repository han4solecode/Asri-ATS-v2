using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.NextStepRule;
using AsriATS.Application.DTOs.Workflow;
using AsriATS.Application.DTOs.WorkflowSequence;

namespace AsriATS.Application.Contracts
{
    public interface IWorkflowService
    {
        Task<BaseResponseDto> CreateWorkflowAsync(WorkflowRequestDto request);
        Task<BaseResponseDto> CreateWorkflowSequenceAsync(WorkflowSequenceRequestDto request);
        Task<BaseResponseDto> CreateNextStepRuleAsync(NextStepRuleRequestDto request);
    }
}
