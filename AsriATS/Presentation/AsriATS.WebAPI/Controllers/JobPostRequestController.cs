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

        /// <summary>
        /// You can submit the job post request here
        /// </summary>
        /// <remarks>
        /// All the parameters in the request body can be null. 
        ///
        ///  
        ///  NOTE: This  will need request body need authorization via bearer token for user roles Recruiter
        ///  
        /// Sample request:
        ///
        ///     POST https://localhost:7080/api/JobPostRequest/create
        ///     {
        ///         "jobTitle": "IT Backend Developer",
        ///         "companyId": 1,
        ///         "description": "Backend Web Developer",
        ///         "requirements": "S-1 Computer Science or any related Major",
        ///         "location": "Jakarta",
        ///         "minSalary": 5000000,
        ///         "maxSalary": 6000000,
        ///         "employmentType": "Permanent",
        ///         "comments": "Job Post For Required position"
        ///     }
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
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

        /// <summary>
        /// You can review the job Post request to approve, rejected, or need modification here
        /// </summary>
        /// /// <remarks>
        /// All the parameters in the request body can be null. 
        ///
        ///  
        ///  NOTE: This Edit will need request body and authorization via bearer token for user with roles HR Manager
        ///  
        /// Sample request:
        ///
        ///     POST https://localhost:7080/api/JobPostRequest/review
        ///     {
        ///         "processId": 1,
        ///         "Action": "Approved",
        ///         "Comment": "Your Job request already added in job post"
        ///     }
        /// </remarks>
        /// <param name="reviewRequest"></param>
        /// <returns></returns>
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

        /// <summary>
        /// You can update your job post request here if the HR Manager status need modification
        /// </summary>
        /// /// <remarks>
        /// All the parameters in the request body can be null. 
        ///
        ///  
        ///  NOTE: This  will need request body need authorization via bearer token for user roles Recruiter
        ///  
        /// Sample request:
        ///
        ///     PUT https://localhost:7080/api/JobPostRequest/update
        ///     {
        ///         "jobTitle": "IT Backend Developer",
        ///         "processId": 1,
        ///         "description": "Backend Web Developer",
        ///         "requirements": "S-1 Computer Science or any related Major",
        ///         "location": "Jakarta",
        ///         "minSalary": 5000000,
        ///         "maxSalary": 6000000,
        ///         "employmentType": "Permanent",
        ///         "comments": "Job Request updated"
        ///     }
        /// </remarks>
        /// <param name="updateJob"></param>
        /// <returns></returns>
        [Authorize(Roles = "Recruiter")]
        [HttpPut("update")]
        public async Task<IActionResult> UpdateJobPostRequest([FromBody] UpdateJobPostRequestDto updateJob)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _jobPostRequestService.UpdateJobPostRequest(updateJob);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }
    }
}
