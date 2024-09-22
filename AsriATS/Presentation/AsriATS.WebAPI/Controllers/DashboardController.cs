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

        public DashboardController(ICompanyService companyService, IUserService userService)
        {
            _companyService = companyService;
            _userService = userService;
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
    }
}