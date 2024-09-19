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

        public DashboardController(ICompanyService companyService)
        {
            _companyService = companyService;
        }
        
        [Authorize(Roles = "Administrator")]
        [HttpGet("company-regist-request")]
        public async Task<IActionResult> GetAllCompanyRegistrationRequest()
        {
            var cr = await _companyService.GetAllCompanyRegisterRequest();

            return Ok(cr);
        }
    }
}