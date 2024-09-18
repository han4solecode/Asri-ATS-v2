using AsriATS.Application.DTOs.Workflow;
using AsriATS.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsriATS.Application.DTOs.WorkflowSequence;

namespace AsriATS.Application.Contracts
{
    public interface IWorkflowSequenceService
    {
        Task<BaseResponseDto> CreateWorkflowSequenceAsync(WorkflowSequenceRequestDto request);
    }
}
