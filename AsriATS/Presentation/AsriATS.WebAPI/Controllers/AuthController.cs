using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Login;
using AsriATS.Application.DTOs.Register;
using AsriATS.Application.DTOs.Update;
using Microsoft.AspNetCore.Authorization;
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
        //[Authorize(Roles = "Applicant, Administrator")]
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

        //update or manage own applicant data
        //[Authorize(Roles = "Applicant, Administrator")]
        [HttpPut("update")]
        public async Task<IActionResult> ManageApplicant([FromBody] UpdateRequestDto update)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var res = await _authService.UpdateApplicantAsync(update);
            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }
            return Ok(res);
        }

        [HttpPost("login")]
        public async Task<IActionResult> LoginAsync([FromBody] LoginRequestDto login)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _authService.LoginAsync(login);
            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }
    }
}
