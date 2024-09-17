using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Register;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        //Create Applicant
        [HttpPost("applicant")]
        public async Task<IActionResult> CreateApplicant([FromBody] RegisterRequestDto register)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var res = await _authService.RegisterApplicantAsync(register);
            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }
            return Ok(res);
        }
    }
}
