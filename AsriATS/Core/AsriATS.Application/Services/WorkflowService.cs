using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Workflow;
using AsriATS.Application.DTOs;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;

namespace AsriATS.Application.Services
{
    public class WorkflowService : IWorkflowService
    {
        private readonly IWorkflowRepository _workflowRepository;
        public WorkflowService (IWorkflowRepository workflowRepository)
        {
            _workflowRepository = workflowRepository;
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
    }
}
