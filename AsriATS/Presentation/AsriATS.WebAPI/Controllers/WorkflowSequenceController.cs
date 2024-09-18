using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.WorkflowSequence;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowSequenceController : ControllerBase
    {
        private readonly IWorkflowSequenceService _workflowSequenceService;

        public WorkflowSequenceController(IWorkflowSequenceService workflowSequenceService)
        {
            _workflowSequenceService = workflowSequenceService;
        }

        // [Authorize(Roles = "Administrator")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateWorkflowSequence([FromBody] WorkflowSequenceRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var res = await _workflowSequenceService.CreateWorkflowSequenceAsync(request);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }
    }
}
