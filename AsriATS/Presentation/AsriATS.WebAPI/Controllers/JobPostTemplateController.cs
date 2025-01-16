using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.JobPost;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobPostTemplateController :ControllerBase
    {
        private readonly IJobPostTemplateService _jobPostTemplateService;
        public JobPostTemplateController(IJobPostTemplateService jobPostTemplateService)
        {
            _jobPostTemplateService = jobPostTemplateService;
        }
        /// <summary>
        /// Retrieves all job post templates based on the user's company.
        /// </summary>
        /// <remarks>
        /// This API endpoint retrieves all job post templates that have already been posted, filtered by the user's company.
        /// 
        /// Only users with the "Recruiter" role are authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/JobPostTemplate/all
        /// 
        /// The response includes a JSON object containing all job post templates belonging to the user's company.
        /// </remarks>
        /// <returns>Returns all job post templates for the user's company.</returns>
        [Authorize(Roles = "Recruiter")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllJobPostTemplates([FromQuery] JobPostSearch jobPostSearch, [FromQuery] Pagination pagination )
        {
            var result = await _jobPostTemplateService.GetAllJobPostTemplate(jobPostSearch,pagination);
            return Ok(result);
        }
        /// <summary>
        /// Retrieves a specific job post template by its ID.
        /// </summary>
        /// <remarks>
        /// This API endpoint allows a "Recruiter" to fetch a specific job post template by providing its ID.
        /// 
        /// The ID must be a valid positive integer. If the ID is invalid, a bad request response will be returned.
        /// 
        /// Note: Only job post templates belonging to the company of the requesting user will be displayed.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/JobPostTemplate/{id}
        /// 
        /// The response includes the details of the job post template.
        /// </remarks>
        /// <param name="id">The ID of the job post template to retrieve.</param>
        /// <returns>Returns the details of the job post template document.</returns>
        [Authorize(Roles = "Recruiter")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobTemplateById(int id)
        {
            var result = await _jobPostTemplateService.GetJobPostTemplate(id);
            if (result.Status == "Error")
            {
                return BadRequest(result.Message);
            }
            return Ok(result);
        }
    }
}
