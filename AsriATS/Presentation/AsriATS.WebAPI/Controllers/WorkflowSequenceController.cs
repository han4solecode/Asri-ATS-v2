using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.WorkflowSequence;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowSequenceController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowSequenceController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        // [Authorize(Roles = "Administrator")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateWorkflowSequence([FromBody] WorkflowSequenceRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var res = await _workflowService.CreateWorkflowSequenceAsync(request);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }
    }
}
