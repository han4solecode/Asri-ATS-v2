using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.JobPostRequest;
using AsriATS.Application.DTOs.Request;
using Microsoft.AspNetCore.Authorization;
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
        [Authorize(Roles = "Recruiter")]
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

        [Authorize(Roles = "HR Manager")]
        [HttpPost("review")]
        public async Task<IActionResult> ReviewJobPostRequest([FromBody] ReviewRequestDto reviewRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _jobPostRequestService.ReviewJobPostRequest(reviewRequest);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }
    }
}
