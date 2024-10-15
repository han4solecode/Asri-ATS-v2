using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Helpers;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class JobPostController : ControllerBase
    {
        private readonly IJobPostService _jobPostService;

        public JobPostController(IJobPostService jobPostService)
        {
            _jobPostService = jobPostService;
        }

        /// <summary>
        /// Retrieves all job posted.
        /// </summary>
        /// <remarks>
        /// This API endpoint retrieves all job that already posted
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/JobPost/all
        /// 
        /// The response includes a JSON object with all job posted.
        /// </remarks>
        /// <returns>Returns all Job Post.</returns>
        [HttpGet("all")]
        public async Task<IActionResult> GetAllJobs([FromQuery]Pagination pagination)
        {
            var result = await _jobPostService.GetAllJobPostAsync(pagination);
            return Ok(result);
        }

        /// <summary>
        /// Search all job listings.
        /// </summary>
        /// <remarks>
        /// This API endpoint search all job that already posted based on some params
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/JobPost/search?Location=Jakarta
        /// 
        /// The response includes a JSON object with all job post.
        /// </remarks>
        /// <returns>Returns all Job Post based on some params.</returns>
        [HttpGet("search")]
        public async Task<IActionResult> SearchJobs([FromQuery]QueryObject queryObject, [FromQuery]Pagination pagination)
        {
            var result = await _jobPostService.SearchJobPostAsync(queryObject, pagination);
            return Ok(result);
        }
    }
}
