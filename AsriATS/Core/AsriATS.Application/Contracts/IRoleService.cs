using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Role;

namespace AsriATS.Application.Contracts
{
    public interface IRoleService
    {
        Task<BaseResponseDto> CreateRoleAsyc(string roleName);
        
        Task<BaseResponseDto> UpdateRoleAsync(RoleUpdateRequestDto roleUpdateRequest);

        Task<BaseResponseDto> DeleteRoleAsync(string roleName);

        Task<BaseResponseDto> AssignRoleAsync(RoleAssignRequestDto roleAssignRequest);

        Task<BaseResponseDto> RevokeRoleAsync(RoleAssignRequestDto roleRevokeRequest);
    }
}