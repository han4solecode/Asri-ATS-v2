using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.ChangePassword;
using AsriATS.Application.DTOs.Update;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Org.BouncyCastle.Asn1.Ocsp;

namespace AsriATS.WebAPI.Controllers
{
    [Route("api/[Controller]")]
    [ApiController]
    public class UserController : ControllerBase
    {
        private readonly IUserService _userService;

        public UserController(IUserService userService)
        {
            _userService = userService;
        }

        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateRequestDto update)
        {
            var res = await _userService.UpdateUserAsync(update);
            return Ok(res);
        }

        [HttpGet("user-info")]
        public async Task<IActionResult> GetUserInfo()
        {
            try
            {
                var user = await _userService.GetUserInfo();
                return Ok(user);
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized("User not logged in.");
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
