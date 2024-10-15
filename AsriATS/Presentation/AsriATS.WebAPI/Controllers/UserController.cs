using AsriATS.Application.Contracts;
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

        /// <summary>
        /// Update current user profile
        /// </summary>
        /// <remarks>
        /// Only user with the role "Administrator", "HR Manager", "Recruiter", and "Applicant" are authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     PUT /api/user/update-user
        ///     {
        ///         "username": "RecruiterCo1",
        ///         "firstname": "Recruiter",
        ///         "lastname": "Co1",
        ///         "email": "frederickzoey90@gmail.com",
        ///         "address": "Jl. Radal, Jakarta",
        ///         "dob": "2002-01-01",
        ///         "sex": "Female",
        ///         "phoneNumber": "+6256789"
        ///     }
        /// 
        /// </remarks>
        /// <param name="update"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Retrieve logged in user info
        /// </summary>
        /// <remarks>
        /// Only user with the role "Administrator", "HR Manager", "Recruiter", and "Applicant" are authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     GET /api/user/user-info
        /// 
        /// </remarks>
        /// <returns>A logged in user data</returns>
        [Authorize(Roles = "Administrator, HR Manager, Recruiter, Applicant")]
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

        /// <summary>
        /// Delete user account
        /// </summary>
        /// <remarks>
        /// Only user with the role "Administrator", "HR Manager", "Recruiter", and "Applicant" are authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     DELETE /api/user/delete/applicant1
        /// 
        /// </remarks>
        /// <param name="username"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator, HR Manager, Recruiter, Applicant")]
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

        /// <summary>
        /// Retrieve all users in the same company
        /// </summary>
        /// <remarks>
        /// Only user with the role "HR Manager" and "Recruiter" are authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     GET /api/user/company
        /// 
        /// </remarks>
        /// <returns></returns>
        [Authorize(Roles = "HR Manager, Recruiter")]
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

        /// <summary>
        /// Upload a new document
        /// </summary>
        /// <remarks>
        /// Only user with the role "Applicant" is authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/user/upload-document
        ///         -F file=@cv.pdf
        /// 
        /// </remarks>
        /// <param name="file"></param>
        /// <returns></returns>
        [Authorize(Roles = "Applicant")]
        [HttpPost("upload-document")]
        public async Task<IActionResult> UploadDocument(IFormFile file)
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

        /// <summary>
        /// Delete an existing document by id
        /// </summary>
        /// <remarks>
        /// Only user with the role "Applicant" is authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     DELETE /api/user/delete-document/4
        /// 
        /// </remarks>
        /// <param name="id"></param>
        /// <returns></returns>
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
