using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.ApplicationJob;
using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.Request;
using AsriATS.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class ApplicationJobController : ControllerBase
    {
        private readonly IApplicationJobService _applicationJobService;

        public ApplicationJobController(IApplicationJobService applicationJobService)
        {
            _applicationJobService = applicationJobService;
        }

        /// <summary>
        /// You can submit the job application here for the user with roles applicant
        /// </summary>
        /// <remarks>
        /// All the parameters in the request body can be null. 
        ///
        ///  You can search by using any of the parameters in the request.
        ///  
        ///  NOTE: This Submit will need request body type multipart/form-data type and authorization via bearer token
        ///  
        /// Sample request:
        ///
        ///     POST https://localhost:7080/api/Workflow/create
        ///     {
        ///        "WorkExperience": "Web Developer in Pacific Bell Inc Inc. (03/2018 – 03/2022)",
        ///        "Education": "S-1 Informatics Engineering",
        ///        "Skills": "Python, Java, Node.js, Ruby on Rails, SQL, RESTful APIs",
        ///        "JobPostId": 1,
        ///        "SupportingDocumentsId": 1,
        ///        "SupportingDocuments: A list of documents like CV, resumes or etc
        ///     }
        /// </remarks>
        /// <param name="applicationJobDto"></param>
        /// <param name="SupportingDocuments"></param>
        /// <returns></returns>
        [HttpPost("SubmitApplication")]
        public async Task<IActionResult> SubmitApplication([FromForm] ApplicationJobDto applicationJobDto, [FromForm] List<IFormFile> SupportingDocuments)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Call the service method with both arguments
            var result = await _applicationJobService.SubmitApplicationJob(applicationJobDto, SupportingDocuments);

            if (result.Status == "Error")
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }

        /// <summary>
        /// Retrieves all job application statuses.
        /// </summary>
        /// <remarks>
        /// This API endpoint is used to fetch the status of all job applications. 
        /// Only users with the roles "Applicant", "HR Manager", or "Recruiter" are authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/job-application
        /// 
        /// The response includes a list of all job applications with their corresponding statuses.
        /// </remarks>
        /// <returns>Returns a list of job application statuses.</returns>
        [Authorize(Roles = "Applicant,HR Manager,Recruiter")]
        [HttpGet("job-application")]
        public async Task<IActionResult> GetAllApplication([FromQuery] ApplicationJobSearchDto searchParams)
        {
            var request = await _applicationJobService.GetApplicationStatusesAsync(searchParams);

            return Ok(request);
        }

        /// <summary>
        /// Retrieves all supporting documents uploaded by the applicant.
        /// </summary>
        /// <remarks>
        /// This API endpoint allows an "Applicant" to retrieve all their uploaded supporting documents, 
        /// such as resumes, cover letters, certifications, etc.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/documents
        /// 
        /// The response includes a list of all supporting documents uploaded by the applicant.
        /// </remarks>
        /// <returns>Returns a list of supporting documents uploaded by the applicant.</returns>
        [Authorize(Roles = "Applicant")]
        [HttpGet("documents")]
        public async Task<IActionResult> GetAllSupportingDocuments()
        {
            var docs = await _applicationJobService.GetAllSupportingDocuments();

            return Ok(docs);
        }

        /// <summary>
        /// Retrieves a specific supporting document by its ID.
        /// </summary>
        /// <remarks>
        /// This API endpoint allows an "Applicant" to fetch a specific supporting document by providing its ID.
        /// 
        /// The ID must be a valid positive integer. If the ID is invalid, a bad request response will be returned.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/documents/{id}
        /// 
        /// The response includes the details of the supporting document.
        /// </remarks>
        /// <param name="id">The ID of the supporting document to retrieve.</param>
        /// <returns>Returns the details of the supporting document.</returns>
        [Authorize(Roles = "Applicant")]
        [HttpGet("documents/{id}")]
        public async Task<IActionResult> GetSupportingDocumentById(int id)
        {
            if (id <= 0)
            {
                return BadRequest("Id must be greater than 0");
            }

            var res = await _applicationJobService.GetSupportingDocumentById(id);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

        /// <summary>
        /// Retrieves all incoming job applications for HR Managers and Recruiters.
        /// </summary>
        /// <remarks>
        /// This API endpoint is used by users with the roles "HR Manager" or "Recruiter" to fetch all incoming job applications.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/incoming-application
        /// 
        /// The response includes a list of all incoming job applications.
        /// </remarks>
        /// <returns>Returns a list of all incoming job applications.</returns>
        [Authorize(Roles = "HR Manager, Recruiter")]
        [HttpGet("incoming-application")]
        public async Task<IActionResult> GetAllIncomingApplications()
        {
            var request = await _applicationJobService.GetAllIncomingApplications();

            return Ok(request);
        }

        /// <summary>
        /// You can review the application job here
        /// </summary>
        /// <remarks>
        /// All the parameters in the request body can be null. 
        ///
        ///  You can search by using any of the parameters in the request.
        ///  
        ///  NOTE: This Review will follow the action based on the workflow that already made
        ///  
        /// Sample request:
        ///
        ///     POST https://localhost:7080/api/ApplicationJob/review
        ///     {
        ///        "ProcessId": 1,
        ///        "Action": "Shortlisted", the status can be like shortlisted, rejected or need additional information
        ///        "Comment": "You're qualified"
        ///     }
        /// </remarks>
        /// <param name="reviewRequest"></param>
        /// <returns></returns>
        [Authorize(Roles = "HR Manager, Recruiter")]
        [HttpPost("review")]
        public async Task<IActionResult> ReviewJobApplication([FromBody] ReviewRequestDto reviewRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _applicationJobService.ReviewJobApplication(reviewRequest);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

        /// <summary>
        /// You can edit the application if the recruiter status need additional information
        /// </summary>
        /// <remarks>
        /// All the parameters in the request body can be null. 
        ///
        ///  You can search by using any of the parameters in the request.
        ///  
        ///  NOTE: This Edit will need request body type multipart/form-data type and authorization via bearer token
        ///  
        /// Sample request:
        ///
        ///     PUT https://localhost:7080/api/ApplicationJob/Update
        ///     {
        ///        "ApplicationJobId": "1",
        ///        "ProcessId": "2",
        ///        "WorkExperience": "Web Developer in Pacific Bell Inc Inc. (03/2018 – 03/2022)",
        ///        "Education": "S-1 Informatics Engineering",
        ///        "Skills": "Python, Java, Node.js, Ruby on Rails, SQL, RESTful APIs",
        ///        "Comments": "",
        ///        "SupportingDocuments: A list of documents like CV, resumes or etc
        ///     }
        /// </remarks>
        /// <param name="request"></param>
        /// <param name="supportingDocuments"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPut("Update")]
        public async Task<IActionResult> UpdateApplicationJob([FromForm] UpdateApplicationJobDto request, [FromForm] List<IFormFile>? supportingDocuments)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Invalid request data"
                });
            }

            var response = await _applicationJobService.UpdateApplicationJob(request, supportingDocuments);

            if (response.Status == "Error")
            {
                return BadRequest(response);
            }

            return Ok(response);
        }

        [Authorize(Roles = "Applicant,HR Manager,Recruiter")]
        [HttpGet("application/{processId}")]
        public async Task<IActionResult> ApplicationDetails(int processId)
        {
            var res = await _applicationJobService.GetProcessAsync(processId);
            return Ok(res);
        }
    }
}
