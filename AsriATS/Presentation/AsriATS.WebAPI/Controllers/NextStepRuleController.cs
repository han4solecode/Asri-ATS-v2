using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.NextStepRule;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class NextStepRuleController : ControllerBase
    {
        private readonly INextStepRuleService _nextStepRuleService;

        public NextStepRuleController(INextStepRuleService nextStepRuleService)
        {
            _nextStepRuleService = nextStepRuleService;
        }

        // [Authorize(Roles = "Administrator")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateNextStepRule([FromBody] NextStepRuleRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var res = await _nextStepRuleService.CreateNextStepRuleAsync(request);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }
    }
}
