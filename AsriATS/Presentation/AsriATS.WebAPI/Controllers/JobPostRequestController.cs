using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.JobPostRequest;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobPostRequestController : ControllerBase
    {
        private readonly IJobPostRequestService _jobPostRequestService;
        public JobPostRequestController(IJobPostRequestService jobPostRequestService)
        {
            _jobPostRequestService = jobPostRequestService;
        }

        [HttpPost("create")]
        public async Task<IActionResult> CreateJobPostRequest([FromBody] JobPostRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var res = await _jobPostRequestService.SubmitJobPostRequest(request);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }
    }
}
