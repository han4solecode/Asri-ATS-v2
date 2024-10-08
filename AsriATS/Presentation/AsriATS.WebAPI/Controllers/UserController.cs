﻿using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.ChangePassword;
using AsriATS.Application.DTOs.Update;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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

        [Authorize(Roles = "Administrator, HR Manager, Recruiter, Applicant")]
        [HttpPut("update-user")]
        public async Task<IActionResult> UpdateUser([FromBody] UpdateRequestDto update)
        {
            try
            {
                var result = await _userService.UpdateUserAsync(update);
                if (result.Status == "Success")
                {
                    return Ok(result);
                }
                else
                {
                    return BadRequest(result);
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Status = "Error", Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
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

        [Authorize(Roles = "Administrator, HR Manager, Recruiter, Applicant")]
        //delete account for user roles hierarchy
        [HttpDelete("delete/{username}")]
        public async Task<IActionResult> DeleteUser(string username)
        {
            try
            {
                var result = await _userService.DeleteUserAsync(username);
                if (result)
                {
                    return Ok(new { Status = "Success", Message = "User deleted successfully." });
                }
                else
                {
                    return BadRequest(new { Status = "Error", Message = "Failed to delete user." });
                }
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(new { Status = "Error", Message = ex.Message });
            }
            catch (Exception ex)
            {
                return BadRequest(new { Status = "Error", Message = ex.Message });
            }
        }

        [HttpGet("company")]
        public async Task<IActionResult> GetUserSameCompany()
        {
            try
            {
                var user = await _userService.GetUsersInSameCompanyAsync();
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

        [Authorize(Roles = "Applicant")]
        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadDocument([FromForm] IFormFile file)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _userService.UploadDocumentAsync(file);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

        [Authorize(Roles = "Applicant")]
        [HttpDelete("delete-document/{id}")]
        public async Task<IActionResult> DeleteDocument(int id)
        {
            if (id <= 0)
            {
                return BadRequest("id must be greater than 0");
            }

            var res = await _userService.DeleteDocumentAsync(id);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }
    }
}
