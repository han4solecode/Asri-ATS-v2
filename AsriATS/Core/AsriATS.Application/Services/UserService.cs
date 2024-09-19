using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.ChangePassword;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Update;
using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.Services
{
    public class UserService:IUserService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        public readonly IHttpContextAccessor _httpContextAccessor;

        public UserService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IHttpContextAccessor httpContextAccessor)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
        }

        // update user info from login user
        public async Task<UpdateResponseDto> UpdateUserAsync(UpdateRequestDto update)
        {
            var users = _httpContextAccessor.HttpContext?.User.Identity!.Name;
            // Find the applicant by username (or you could use user ID)
            var user = await _userManager.FindByNameAsync(users);

            if (user == null)
            {
                return new UpdateResponseDto
                {
                    Status = "Error",
                    Message = "Applicant not found!"
                };
            }

            // Update the fields that are allowed to be modified
            user.UserName = update.Username;
            user.Email = update.Email;
            user.PhoneNumber = update.PhoneNumber;
            user.FirstName = update.Firstname;
            user.LastName = update.Lastname;
            user.Address = update.Address;
            user.Dob = update.Dob;
            user.Sex = update.Sex;

            // Save the changes
            var result = await _userManager.UpdateAsync(user);

            if (!result.Succeeded)
            {
                return new UpdateResponseDto
                {
                    Status = "Error",
                    Message = "Applicant update failed! Please check the details and try again."
                };
            }

            return new UpdateResponseDto
            {
                Status = "Success",
                Message = "Applicant details updated successfully."
            };
        }


        // Get user info by login
        public async Task<object> GetUserInfo()
        {
            // Get the username of the currently logged-in user
            var userName = _httpContextAccessor.HttpContext?.User.Identity!.Name;

            if (userName == null)
            {
                throw new UnauthorizedAccessException("User is not logged in.");
            }

            // Find the user by username (or you could use user ID if needed)
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                throw new Exception("User not found.");
            }

            return user;
        }

        // change password
        public async Task<BaseResponseDto> ChangePasswordAsync(ChangePasswordRequestDto request)
        {
            var users = _httpContextAccessor.HttpContext?.User.Identity!.Name;
            // Find the applicant by username (or you could use user ID)
            var user = await _userManager.FindByNameAsync(users);

            if (user == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "User not found!"
                };
            }

            // Generate a password reset token
            var resetToken = await _userManager.GeneratePasswordResetTokenAsync(user);

            // Change the user's password
            var result = await _userManager.ResetPasswordAsync(user, resetToken, request.NewPassword);

            if (!result.Succeeded)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Password change failed! Please check the details and try again."
                };
            }

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Password changed successfully."
            };
        }
    }
}
