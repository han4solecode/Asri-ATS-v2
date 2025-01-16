using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.JobPost;
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
        private readonly IDashboardService _dashboardService;

        public DashboardController(ICompanyService companyService, IUserService userService, IRoleService roleService, IJobPostRequestService jobPostRequestService, IJobPostTemplateRequestService jobPostTemplateRequestService, IDashboardService dashboardService)
        {
            _companyService = companyService;
            _userService = userService;
            _roleService = roleService;
            _jobPostRequestService = jobPostRequestService;
            _jobPostTemplateRequestService = jobPostTemplateRequestService;
            _dashboardService = dashboardService;
        }
        
        /// <summary>
        /// Retrieve all incoming company registration request
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/dashboard/company-regist-request
        /// 
        /// </remarks>
        /// <returns>A list of company registration request</returns>
        [Authorize(Roles = "Administrator")]
        [HttpGet("company-regist-request")]
        public async Task<IActionResult> GetAllCompanyRegistrationRequest()
        {
            var cr = await _companyService.GetAllCompanyRegisterRequest();

            return Ok(cr);
        }

        /// <summary>
        /// Retrieve all user info
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/dashboard/users
        /// 
        /// </remarks>
        /// <returns>A list of all users</returns>
        [Authorize(Roles = "Administrator")]
        [HttpGet("users")]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _userService.GetAllUserInfoAsync();

            return Ok(users);
        }

        /// <summary>
        /// Retrieve all role change requests
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/dashboard/role-change-request
        /// 
        /// </remarks>
        /// <returns>A list of role change request</returns>
        [Authorize(Roles = "Administrator")]
        [HttpGet("role-change-request")]
        public async Task<IActionResult> GetAllRoleChangeRequest()
        {
            var rcr = await _roleService.GetAllRoleChangeRequest();

            return Ok(rcr);
        }

        /// <summary>
        /// Retreive all job post request in a company
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/dashboard/job-post-request
        /// 
        /// </remarks>
        /// <returns>A list of job post request</returns>
        [Authorize(Roles = "HR Manager")]
        [HttpGet("job-post-request")]
        public async Task<IActionResult> GetAllJobPostRequestToReview([FromQuery] JobPostSearch queryObject, [FromQuery] Pagination pagination)
        {
            var request = await _jobPostRequestService.GetJobPostRequestToReview(queryObject,pagination);

            return Ok(request);
        }

        /// <summary>
        /// Retreive all job post template request in a company
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     GET /api/dashboard/job-post-template-request
        /// 
        /// </remarks>
        /// <returns>A list of job post template request</returns>
        [Authorize(Roles = "HR Manager")]
        [HttpGet("job-post-template-request")]
        public async Task<IActionResult> GetAllJobPostTemplateRequestToReview()
        {
            var request = await _jobPostTemplateRequestService.GetAllJobPostTemplateRequest(null,null);

            return Ok(request);
        }

        [Authorize(Roles = "Applicant")]
        [HttpGet("applicant-dashboard")]
        public async Task<IActionResult> GetApplicantDashboard()
        {
            var req = await _dashboardService.GetApplicantDashboard();
            return Ok(req);
        }

        [Authorize(Roles = "Recruiter")]
        [HttpGet("recruiter-dashboard")]
        public async Task<IActionResult> GetRecruiterDashboard()
        {
            var req = await _dashboardService.GetRecruiterDashboard();
            return Ok(req);
        }

        [Authorize(Roles = "HR Manager")]
        [HttpGet("HRManager-dashboard")]
        public async Task<IActionResult> GetHRDashboard()
        {
            var req = await _dashboardService.GetHRManagerDashboard();
            return Ok(req);
        }
    }
}