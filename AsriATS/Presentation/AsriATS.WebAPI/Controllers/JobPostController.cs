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

        [HttpGet("all")]
        public async Task<IActionResult> GetAllJobs([FromQuery]Pagination pagination)
        {
            var result = await _jobPostService.GetAllJobPostAsync(pagination);
            return Ok(result);
        }

        [HttpGet("search")]
        public async Task<IActionResult> SearchJobs([FromQuery]QueryObject queryObject, [FromQuery]Pagination pagination)
        {
            var result = await _jobPostService.SearchJobPostAsync(queryObject, pagination);
            return Ok(result);
        }
    }
}
