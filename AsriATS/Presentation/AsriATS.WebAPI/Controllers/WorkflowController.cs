using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Workflow;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class WorkflowController : ControllerBase
    {
        private readonly IWorkflowService _workflowService;

        public WorkflowController(IWorkflowService workflowService)
        {
            _workflowService = workflowService;
        }

        /// <summary>
        /// You can Create for the Workflow here
        /// </summary>
        /// <remarks>
        /// All the parameters in the request body can be null. 
        ///
        ///  You can search by using any of the parameters in the request.
        ///  
        ///  NOTE: This workflow can use for create job post and application job
        ///  
        /// Sample request:
        ///
        ///     POST https://localhost:7080/api/Workflow/create
        ///     {
        ///        "workflowName": "Job Post Request",
        ///        "description": "Workflow for create a job post request"
        ///     }
        ///     OR
        ///     
        ///     POST https://localhost:7080/api/Workflow/create
        ///     {
        ///        "workflowName": "Application Job Request",
        ///        "description": "Workflow for create a application job request"
        ///     }
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
        //[Authorize(Roles = "Administrator")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateWorkflow([FromBody] WorkflowRequestDto request)
        {
            var res = await _workflowService.CreateWorkflowAsync(request);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }
    }
}
