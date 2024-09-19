using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Company;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> RegisterCompany([FromBody] CompanyRegisterRequestDto companyRegisterRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _companyService.CompanyRegisterRequestAsync(companyRegisterRequest);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

        [HttpPost("review-request")]
        public async Task<IActionResult> ReviewCompanyRegisterRequest([FromBody] CompanyRegisterReviewDto companyRegisterReview)
        {
             if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _companyService.ReviewCompanyRegisterRequest(companyRegisterReview);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }
    }
}