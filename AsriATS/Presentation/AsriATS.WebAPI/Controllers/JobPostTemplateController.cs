using AsriATS.Application.Contracts;
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
        [Authorize(Roles = "Recruiter")]
        [HttpGet("all")]
        public async Task<IActionResult> GetAllJobPostTemplates()
        {
            var result = await _jobPostTemplateService.GetAllJobPostTemplate();
            return Ok(result);
        }
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
