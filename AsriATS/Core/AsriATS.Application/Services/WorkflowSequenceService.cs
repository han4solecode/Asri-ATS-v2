using AsriATS.Application.DTOs.Workflow;
using AsriATS.Application.DTOs;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.WorkflowSequence;

namespace AsriATS.Application.Services
{
    public class WorkflowSequenceService : IWorkflowSequenceService
    {
        private readonly IWorkflowSequenceRepository _workflowSequenceRepository;
        private readonly IWorkflowRepository _workflowRepository;
        public WorkflowSequenceService(IWorkflowSequenceRepository workflowSequenceRepository, IWorkflowRepository workflowRepository)
        {
            _workflowSequenceRepository = workflowSequenceRepository;
            _workflowRepository = workflowRepository;
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
    }
}
