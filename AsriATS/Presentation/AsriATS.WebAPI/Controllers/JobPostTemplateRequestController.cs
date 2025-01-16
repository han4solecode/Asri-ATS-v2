using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.JobPost;
using AsriATS.Application.DTOs.JobPostRequest;
using AsriATS.Application.DTOs.JobPostTemplateRequest;
using AsriATS.Application.Services;
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
        /// <summary>
        /// Submits a job post template request.
        /// </summary>
        /// <remarks>
        /// All parameters in the request body must be provided and cannot be null.
        /// 
        /// Note: This operation requires authorization via a bearer token, and is only accessible to users with the "Recruiter" role.
        /// Additionally, only recruiters from the company specified in the request are allowed to submit a job post template request.
        /// 
        /// Sample request:
        /// 
        ///     POST https://localhost:7080/api/JobPostTemplateRequest/create
        ///     {
        ///         "jobTitle": "IT Backend Developer",
        ///         "companyId": 1,
        ///         "description": "Backend Web Developer",
        ///         "requirements": "S-1 Computer Science or any related major",
        ///         "location": "Jakarta",
        ///         "minSalary": 5000000,
        ///         "maxSalary": 6000000,
        ///         "employmentType": "Permanent"
        ///     }
        /// 
        /// The response includes a confirmation of the job post template submission.
        /// </remarks>
        /// <param name="request">The job post template request to submit.</param>
        /// <returns></returns>
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
        /// <summary>
        /// You can review the Job Post Template request to approve or rejected
        /// </summary>
        /// <remarks>
        /// All the parameters in the request body cannot be null. 
        ///  
        ///  NOTE: This Edit will need request body and authorization via bearer token for user with roles HR Manager
        ///  Additionally, only HR Manager from the company specified in the request are allowed to review a job post template request.
        ///
        /// Sample request:
        ///
        ///     POST https://localhost:7080/api/JobPostTemplateRequest/review
        ///     {
        ///         "JobTemplateRequestId": 1,
        ///         "Action": "Approved"
        ///     }
        /// </remarks>
        /// <param name="jobPostTemplateReview"></param>
        /// <returns></returns>
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

        [Authorize(Roles = "Applicant,HR Manager,Recruiter")]
        [HttpGet]
        public async Task<IActionResult> GetAllJobPostTemplateRequests([FromQuery] JobPostSearch jobPostSearch, [FromQuery] Pagination pagination)
        {
            var res = await _jobPostTemplateRequestService.GetAllJobPostTemplateRequest(jobPostSearch, pagination);
            return Ok(res);
        }

        [Authorize(Roles = "Applicant,HR Manager,Recruiter")]
        [HttpGet("{id}")]
        public async Task<IActionResult> GetJobPostTemplateRequest(int id)
        {
            var res = await _jobPostTemplateRequestService.GetJobPostTemplateRequest(id);
            return Ok(res);
        }
    }
}
