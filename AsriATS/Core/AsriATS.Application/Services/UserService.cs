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
using System.Security.Claims;

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

        // update user based on login user and roles
        public async Task<UpdateResponseDto> UpdateUserAsync(UpdateRequestDto update)
        {
            // Get the username of the currently logged-in user
            var currentUser = _httpContextAccessor.HttpContext?.User.Identity!.Name;

            if (currentUser == null)
            {
                return new UpdateResponseDto
                {
                    Status = "Error",
                    Message = "User is not logged in."
                };
            }

            // Get the roles of the currently logged-in user
            var currentUserRoles = _httpContextAccessor.HttpContext?.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Find the user to be updated by username
            var userToUpdate = await _userManager.FindByNameAsync(update.Username);

            if (userToUpdate == null)
            {
                return new UpdateResponseDto
                {
                    Status = "Error",
                    Message = "User not found!"
                };
            }

            // Check if the current user has the rights to update the target user
            if (currentUserRoles.Contains("Administrator"))
            {
                // Administrator can update anyone
                UpdateUserFields(userToUpdate, update);
            }
            else if (currentUserRoles.Contains("HR Manager"))
            {
                if (update.Username == currentUser || await _userManager.IsInRoleAsync(userToUpdate, "Recruiter"))
                {
                    // HR Manager can update their own details or those of recruiters
                    UpdateUserFields(userToUpdate, update);
                }
                else
                {
                    return new UpdateResponseDto
                    {
                        Status = "Error",
                        Message = "HR Manager can only update their own account or recruiters."
                    };
                }
            }
            else if (currentUserRoles.Contains("Recruiter"))
            {
                if (update.Username == currentUser)
                {
                    // Recruiter can only update their own account
                    UpdateUserFields(userToUpdate, update);
                }
                else
                {
                    return new UpdateResponseDto
                    {
                        Status = "Error",
                        Message = "Recruiter can only update their own account."
                    };
                }
            }
            else if (currentUserRoles.Contains("Applicant"))
            {
                if (update.Username == currentUser)
                {
                    // Applicant can only update their own account
                    UpdateUserFields(userToUpdate, update);
                }
                else
                {
                    return new UpdateResponseDto
                    {
                        Status = "Error",
                        Message = "Applicant can only update their own account."
                    };
                }
            }
            else
            {
                return new UpdateResponseDto
                {
                    Status = "Error",
                    Message = "User does not have permission to update this account."
                };
            }

            // Save the changes
            var result = await _userManager.UpdateAsync(userToUpdate);

            if (!result.Succeeded)
            {
                return new UpdateResponseDto
                {
                    Status = "Error",
                    Message = "User update failed! Please check the details and try again."
                };
            }

            return new UpdateResponseDto
            {
                Status = "Success",
                Message = "User details updated successfully."
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

            // Get the roles associated with the user
            var roles = await _userManager.GetRolesAsync(user);

            // Return the user information along with roles
            return new
            {
                userId = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                Password  = user.SecurityStamp,
                PhoneNumber = user.PhoneNumber,
                FirstName = user.FirstName,
                LastName = user.LastName,
                Address = user.Address,
                Dob = user.Dob,
                Sex = user.Sex,
                Roles = roles
            };
        }

        // delete user based on login and user roles
        public async Task<bool> DeleteUserAsync(string userName)
        {
            // Get the username of the currently logged-in user
            var currentUser = _httpContextAccessor.HttpContext?.User.Identity!.Name;

            if (currentUser == null)
            {
                throw new UnauthorizedAccessException("User is not logged in.");
            }

            // Get the roles of the currently logged-in user
            var currentUserRoles = _httpContextAccessor.HttpContext?.User.Claims
                .Where(c => c.Type == ClaimTypes.Role)
                .Select(c => c.Value)
                .ToList();

            // Find the user to be deleted
            var userToDelete = await _userManager.FindByNameAsync(userName);

            if (userToDelete == null)
            {
                throw new Exception("User not found.");
            }

            // Ensure user can only delete their own account or has proper permissions
            if (currentUserRoles.Contains("Administrator"))
            {
                // Administrator can delete anyone
                await _userManager.DeleteAsync(userToDelete);
            }
            else if (currentUserRoles.Contains("HR Manager"))
            {
                if (userName == currentUser || await _userManager.IsInRoleAsync(userToDelete, "Recruiter"))
                {
                    // HR Manager can delete their own account or recruiter accounts
                    await _userManager.DeleteAsync(userToDelete);
                }
                else
                {
                    throw new UnauthorizedAccessException("HR Manager can only delete their own account or recruiters.");
                }
            }
            else if (currentUserRoles.Contains("Recruiter"))
            {
                if (userName == currentUser)
                {
                    // Recruiter can only delete their own account
                    await _userManager.DeleteAsync(userToDelete);
                }
                else
                {
                    throw new UnauthorizedAccessException("Recruiter can only delete their own account.");
                }
            }
            else if (currentUserRoles.Contains("Applicant"))
            {
                if (userName == currentUser)
                {
                    // Applicant can only delete their own account
                    await _userManager.DeleteAsync(userToDelete);
                }
                else
                {
                    throw new UnauthorizedAccessException("Applicant can only delete their own account.");
                }
            }
            else
            {
                throw new UnauthorizedAccessException("User does not have permission to delete this account.");
            }

            return true;
        }

        private void UpdateUserFields(AppUser user, UpdateRequestDto update)
        {
            // Update the fields that are allowed to be modified
            user.UserName = update.Username;
            user.Email = update.Email;
            user.PhoneNumber = update.PhoneNumber;
            user.FirstName = update.Firstname;
            user.LastName = update.Lastname;
            user.Address = update.Address;
            user.Dob = update.Dob;
            user.Sex = update.Sex;
        }
    }
}
