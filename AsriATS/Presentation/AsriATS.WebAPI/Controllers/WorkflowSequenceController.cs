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
        /// <summary>
        /// You can Create for the Workflow Sequence here
        /// </summary>
        /// <remarks>
        /// 
        /// All the parameters in the request body cannot be null except requiredRole. 
        ///  
        /// NOTE: The `Workflow Sequence` is used to define the sequence of steps in a process.
        /// For example, a `Workflow Sequence` can be used to define the stages in the process of creating a job post or submitting a job application.
        /// Sample request:
        ///
        ///     POST https://localhost:7080/api/WorkflowSequence/create
        ///     {
        ///         "workflowId": 1,
        ///         "stepOrder": 1,
        ///         "stepName": "Submit Job Application",
        ///         "requiredRole": "9c973a4a-7974-4b43-9731-8c318accf936"
        ///     }
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
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
