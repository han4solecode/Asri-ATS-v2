using AsriATS.Application.Contracts;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DashboardController : ControllerBase
    {
        private readonly ICompanyService _companyService;
        private readonly IUserService _userService;
        private readonly IRoleService _roleService;
        private readonly IJobPostRequestService _jobPostRequestService;
        private readonly IJobPostTemplateRequestService _jobPostTemplateRequestService;

        public DashboardController(ICompanyService companyService, IUserService userService, IRoleService roleService, IJobPostRequestService jobPostRequestService, IJobPostTemplateRequestService jobPostTemplateRequestService)
        {
            _companyService = companyService;
            _userService = userService;
            _roleService = roleService;
            _jobPostRequestService = jobPostRequestService;
            _jobPostTemplateRequestService = jobPostTemplateRequestService;
        }
        
        [Authorize(Roles = "Administrator")]
        [HttpGet("company-regist-request")]
        public async Task<IActionResult> GetAllCompanyRegistrationRequest()
        {
            var cr = await _companyService.GetAllCompanyRegisterRequest();

            return Ok(cr);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUserInfoAsync();

            return Ok(users);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("role-change-request")]
        public async Task<IActionResult> GetAllRoleChangeRequest()
        {
            var rcr = await _roleService.GetAllRoleChangeRequest();

            return Ok(rcr);
        }

        [Authorize(Roles = "HR Manager, Recruiter")]
        [HttpGet("job-post-request")]
        public async Task<IActionResult> GetAllJobPostRequestToReview()
        {
            var request = await _jobPostRequestService.GetJobPostRequestToReview();

            return Ok(request);
        }

        [Authorize(Roles = "HR Manager")]
        [HttpGet("job-post-template-request")]
        public async Task<IActionResult> GetAllJobPostTemplateRequestToReview()
        {
            var request = await _jobPostTemplateRequestService.GetAllJobPostTemplateRequestToReview();

            return Ok(request);
        }
    }
}