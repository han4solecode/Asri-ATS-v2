using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.ChangePassword;
using AsriATS.Application.DTOs.Login;
using AsriATS.Application.DTOs.Register;
using AsriATS.Application.DTOs.Update;
using AsriATS.Application.Services;
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

        /// <summary>
        /// Create a new applicant user
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/applicant
        ///     {
        ///         "username": "applicant2",
        ///         "firstname": "Bang",
        ///         "lastname": "Rondo",
        ///         "email": "bangrondo@gmail.com",
        ///         "password": "Ab123456",
        ///         "address": "Jl. Tembus, Jakarta",
        ///         "dob": "2002-02-22",
        ///         "sex": "Male",
        ///         "phoneNumber": "0811437661"
        ///     }
        /// 
        /// </remarks>
        /// <param name="register"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Create a new user with a specific role
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/create-user?rolename={rolename}
        ///     {
        ///         "username": "admin",
        ///         "firstname": "Admin",
        ///         "lastname": "Cool",
        ///         "email": "admin@gmail.com",
        ///         "password": "Ab123456",
        ///         "address": "Jl Ngasal, Bandung",
        ///         "dob": "1998-02-11",
        ///         "sex": "Female",
        ///         "phoneNumber": "0814018739"
        ///     }
        /// 
        /// </remarks>
        /// <param name="register"></param>
        /// <param name="rolename"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost("create-user")]
        public async Task<IActionResult> CreateUser([FromBody] RegisterUserRequestDto register, string rolename)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }
            var res = await _authService.RegisterUserAsync(register, rolename);
            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }
            return Ok(res);
        }

        [Authorize(Roles = "Applicant, Administrator")]
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

        /// <summary>
        /// Login a user account
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/login
        ///     {
        ///         "username": "applicant1",
        ///         "password": "Ab123456"
        ///     }
        /// 
        /// </remarks>
        /// <param name="login"></param>
        /// <returns></returns>
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
                return BadRequest(res);
            }

            SetRefreshTokenCookie("AuthToken", res.AccessToken, res.AccessTokenExpiryTime);
            SetRefreshTokenCookie("RefreshToken", res.RefreshToken, res.RefreshTokenExpiryTime);

            return Ok(res);
        }

        [Authorize]
        [HttpPost("logout")]
        public async Task<IActionResult> LogoutAsync()
        {
            var result = await _authService.LogoutAsync();

            if (result.Status == "Error")
            {
                return BadRequest(result);
            }

            try
            {
                Response.Cookies.Delete("AuthToken", new CookieOptions
                {
                    HttpOnly = true,
                    Secure = true,
                    SameSite = SameSiteMode.Strict,
                });

                return Ok(result);
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, $"An error occured during logout. Message: {ex}");
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshAccessToken()
        {
            var refreshToken = Request.Cookies["RefreshToken"];

            if (refreshToken == null)
            {
                try
                {

                    Response.Cookies.Delete("AuthToken", new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                    });

                    Response.Cookies.Delete("RefreshToken", new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true,
                        SameSite = SameSiteMode.Strict,
                    });

                    var res = new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "Refresh token expired"
                    };

                    return BadRequest(res);
                }
                catch (System.Exception ex)
                {
                    return StatusCode(500, $"An error occured. Message: {ex}");
                }
            }

            var result = await _authService.RefreshAccessTokenAsync(refreshToken!);

            if (result.Status == "Error")
            {
                return BadRequest(result);
            }

            SetRefreshTokenCookie("AuthToken", result.AccessToken, result.AccessTokenExpiryTime);
            SetRefreshTokenCookie("RefreshToken", result.RefreshToken, result.RefreshTokenExpiryTime);

            return Ok(result);
        }

        /// <summary>
        /// Change user password
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/auth/changepassword
        ///     {
        ///         "newPassword": "Ab123456"
        ///     }
        ///     
        /// </remarks>
        /// <param name="change"></param>
        /// <returns></returns>
        [Authorize]
        [HttpPost("ChangePassword")]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto change)
        {
            var result = await _authService.ChangePasswordAsync(change);
            if (result.Status == "Error")
            {
                return BadRequest(result.Message);
            }

            return Ok(result.Message);
        }

        private void SetRefreshTokenCookie(string tokenType, string? token, DateTime? expires)
        {
            var cookieOptions = new CookieOptions
            {
                HttpOnly = true,
                Secure = true,
                SameSite = SameSiteMode.Strict,
                Expires = expires,
            };

            Response.Cookies.Append(tokenType, token!, cookieOptions);
        }
    }
}
