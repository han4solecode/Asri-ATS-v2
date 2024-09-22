using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Role;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AsriATS.Application.Services
{
    public class RoleService : IRoleService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IRoleChangeRequestRepository _roleChangeRequestRepository;

        public RoleService(UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IHttpContextAccessor httpContextAccessor, IRoleChangeRequestRepository roleChangeRequestRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _httpContextAccessor = httpContextAccessor;
            _roleChangeRequestRepository = roleChangeRequestRepository;
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

        public async Task<BaseResponseDto> RoleChangeRequestAsync(string requestedRole)
        {
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);

            // check user role. if role == requestedRole, return error
            var userRole = await _userManager.GetRolesAsync(user!);

            if (userRole.Contains(requestedRole))
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = $"User already in {requestedRole} role"
                };
            }

            try
            {
                var newRoleChangeRequest = new RoleChangeRequest
                {
                    UserId = user!.Id,
                    CurrentRole = userRole.Single(),
                    RequestedRole = requestedRole
                };

                var rcr = await _roleChangeRequestRepository.GetAllAsync();

                if (rcr.Where(x => x.IsApproved == null).Any(x => x.UserId == user.Id))
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "Already requested a role change request. Please wait"
                    };
                }

                await _roleChangeRequestRepository.CreateAsync(newRoleChangeRequest);

                // send email notification to user.Email and admin

                
                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Role Change Requested Successfuly"
                };
            }
            catch (System.Exception)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Role Change Request Error"
                };
            }
        }

        public async Task<BaseResponseDto> ReviewRoleChangeRequest(RoleChangeRequestDto roleChangeRequest)
        {
            var request = await _roleChangeRequestRepository.GetByIdAsync(roleChangeRequest.RoleChangeRequestId);

            if (request == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Role change request does not exist"
                };
            }

            if (roleChangeRequest.Action == "Approved")
            {
                // update request
                request.IsApproved = true;
                await _roleChangeRequestRepository.UpdateAsync(request);

                // get requester AppUser
                var user = await _userManager.FindByIdAsync(request.UserId);

                // get user current role
                var userRole = request.CurrentRole;

                // revoke role
                await _userManager.RemoveFromRoleAsync(user!, userRole);

                // assign new role
                await _userManager.AddToRoleAsync(user!, request.RequestedRole);

                // send email to user.Email


                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Role change request approved successfuly"
                };
            }
            else if (roleChangeRequest.Action == "Rejected")
            {
                // update request
                request.IsApproved = true;
                await _roleChangeRequestRepository.UpdateAsync(request);

                // send email to user.Email
                

                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Role change request rejeted sucessfuly"
                };
            }
            else
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Action not available"
                };
            }
        }
    }
}