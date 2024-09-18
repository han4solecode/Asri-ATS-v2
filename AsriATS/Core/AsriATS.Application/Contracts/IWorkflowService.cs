using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Workflow;

namespace AsriATS.Application.Contracts
{
    public interface IWorkflowService
    {
        Task<BaseResponseDto> CreateWorkflowAsync(WorkflowRequest request);
    }
}
