using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Role;
using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace AsriATS.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public RoleService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        public async Task<BaseResponseDto> AssignRoleAsync(RoleAssignRequestDto roleAssignRequest)
        {
            var user = await _userManager.FindByIdAsync(roleAssignRequest.AppUserId);

            if (user == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "User not found"
                };
            }

            if (!await _roleManager.RoleExistsAsync(roleAssignRequest.RoleName))
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Role not found"
                };
            }

            await _userManager.AddToRoleAsync(user, roleAssignRequest.RoleName);

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Role assigned successfully"
            };
        }

        public async Task<BaseResponseDto> CreateRoleAsyc(string roleName)
        {
            if (!await _roleManager.RoleExistsAsync(roleName))
            {
                var role = new AppRole()
                {
                    Name = roleName
                };

                await _roleManager.CreateAsync(role);

                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Role created successfully"
                };
            }
            else
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Role already exist"
                };
            }
        }

        public async Task<BaseResponseDto> DeleteRoleAsync(string roleName)
        {
            var role = await _roleManager.FindByNameAsync(roleName);

            if (role == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Role does not exist"
                };
            }

            await _roleManager.DeleteAsync(role);

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Role deleted successfully"
            };
        }

        public async Task<BaseResponseDto> RevokeRoleAsync(RoleAssignRequestDto roleRevokeRequest)
        {
            var user = await _userManager.FindByIdAsync(roleRevokeRequest.AppUserId);

            if (user == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "User not found"
                };
            }

            var userRoles = await _userManager.GetRolesAsync(user);

            var isRoleExist = userRoles.Any(r => r == roleRevokeRequest.RoleName);

            if (!isRoleExist)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = $"User is not in {roleRevokeRequest.RoleName} role"
                };
            }

            await _userManager.RemoveFromRoleAsync(user, roleRevokeRequest.RoleName);

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Role revoked successfully"
            };
        }

        public async Task<BaseResponseDto> UpdateRoleAsync(RoleUpdateRequestDto roleUpdateRequest)
        {
            var roleToEdit = await _roleManager.FindByNameAsync(roleUpdateRequest.RoleName);

            if (roleToEdit == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Role does not exist"
                };
            }

            if (roleToEdit.Name != roleUpdateRequest.RoleName)
            {
                roleToEdit.Name = roleUpdateRequest.RoleName;
            }

            await _roleManager.UpdateAsync(roleToEdit);

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Role updated successfully"
            };
        }
    }
}