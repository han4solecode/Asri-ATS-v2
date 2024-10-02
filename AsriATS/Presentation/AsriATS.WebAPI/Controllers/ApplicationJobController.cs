using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.ApplicationJob;
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

        [Authorize(Roles = "Applicant, HR Manager, Recruiter")]
        [HttpGet("job-application")]
        public async Task<IActionResult> GetAllApplication()
        {
            var request = await _applicationJobService.GetAllApplicationStatuses();

            return Ok(request);
        }

        [Authorize(Roles = "Applicant")]
        [HttpGet("documents")]
        public async Task<IActionResult> GetAllSupportingDocuments()
        {
            var docs = await _applicationJobService.GetAllSupportingDocuments();

            return Ok(docs);
        }

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

        [Authorize(Roles = "HR Manager, Recruiter")]
        [HttpGet("incoming-application")]
        public async Task<IActionResult> GetAllIncomingApplications()
        {
            var request = await _applicationJobService.GetAllIncomingApplications();

            return Ok(request);
        }
    }
}
