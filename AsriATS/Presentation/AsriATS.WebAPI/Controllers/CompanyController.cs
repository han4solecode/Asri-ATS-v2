using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Company;
using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// You can submit the company register request and add HR Manager account for that company
        /// </summary>
        /// <remarks>
        /// All the parameters in the request body can be null. 
        ///
        ///  You can search by using any of the parameters in the request.
        ///  
        ///  NOTE: You must make sure the email address is right because it will send the username and password via email for the login
        ///  
        /// Sample request:
        ///
        ///     POST https://localhost:7080/api/company/register
        ///     {
        ///         "CompanyName": "Test 2",
        ///         "CompanyAddress": "Jl Pisok",
        ///         "Email": "risyadkamarullah2019@gmail.com",
        ///         "FirstName": "Farhan",
        ///         "LastName": "Atha",
        ///         "UserAddress": "Jl. Buntu",
        ///         "Dob": "2001-07-02",
        ///         "Sex": "Male",
        ///         "PhoneNumber": "081381818181"
        ///     }
        /// </remarks>
        /// <param name="companyRegisterRequest"></param>
        /// <returns></returns>
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
                return BadRequest(res);
            }

            return Ok(res);
        }

        /// <summary>
        ///  You can review Company Register the request for Add Company and HR Manager Account
        /// </summary>
        /// <remarks>
        /// All the parameters in the request body can be null. 
        ///
        ///  You can search by using any of the parameters in the request.
        ///  
        ///  NOTE: You must make sure the company request Id is right
        ///  
        /// Sample request:
        ///
        ///     POST https://localhost:7080/api/company/review-request
        ///     {
        ///         "companyRequestId": 3,
        ///         "action": "Approved"
        ///     }
        /// </remarks>
        /// <param name="companyRegisterReview"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
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
                return BadRequest(res);
            }

            return Ok(res);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("requests")]
        public async Task<IActionResult> GetAllCompanyRegistrationRequest()
        {
            var res = await _companyService.GetAllCompanyRegisterRequest();

            return Ok(res);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("request/{id}")]
        public async Task<IActionResult> GetCompanyRegistrationRequestById(int id)
        {
            var res = await _companyService.GetCompanyRegistrationRequestById(id);

            return Ok(res);
        }

        [HttpGet("company")]
        public async Task<IActionResult> GetCompany()
        {
            var res = await _companyService.GetAllCompany();
            return Ok(res);
        }
    }
}