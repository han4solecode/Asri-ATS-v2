using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.NextStepRule;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NextStepRuleController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;

        public NextStepRuleController(IWorkflowService nextStepRuleService)
        {
            _workflowService = nextStepRuleService;
        }

        // [Authorize(Roles = "Administrator")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateNextStepRule([FromBody] NextStepRuleRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var res = await _workflowService.CreateNextStepRuleAsync(request);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }
    }
}
