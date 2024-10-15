using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.NextStepRule;
using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// Create a new next step rule 
        /// </summary>
        /// <param name="request"></param>
        /// <remarks>
        /// Only user with the role "Administrator" is authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/NextStepRule/create
        ///     {
        ///         "CurrentStepId": 3,
        ///         "NextStepId": 4,
        ///         "ConditionType": "Action",
        ///         "ConditionValue": "Approved"
        ///     }
        /// 
        /// </remarks>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
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
