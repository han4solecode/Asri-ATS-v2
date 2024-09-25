using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.JobPostRequest;
using AsriATS.Application.DTOs.JobPostTemplateRequest;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobPostTemplateRequestController : ControllerBase
    {
        private readonly IJobPostTemplateRequestService _jobPostTemplateRequestService;

        public JobPostTemplateRequestController(IJobPostTemplateRequestService jobPostTemplateRequestService)
        {
            _jobPostTemplateRequestService = jobPostTemplateRequestService;
        }

        [Authorize(Roles = "Recruiter")]
        [HttpPost("create")]
        public async Task<IActionResult> CreateJobPostRequest([FromBody] JobPostTemplateRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var res = await _jobPostTemplateRequestService.SubmitJobTemplateRequest(request);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

        [Authorize(Roles = "HR Manager")]
        [HttpPost("review")]
        public async Task<IActionResult> ReviewJobPostTemplateRequest([FromBody] JobPostTemplateReviewDto jobPostTemplateReview)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _jobPostTemplateRequestService.ReviewJobPostTemplateRequest(jobPostTemplateReview);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }
    }
}
