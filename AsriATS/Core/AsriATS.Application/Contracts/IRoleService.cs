using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Role;
using AsriATS.Domain.Entities;

namespace AsriATS.Application.Contracts
{
    public interface IRoleService
    {
        Task<BaseResponseDto> CreateRoleAsyc(string roleName);
        
        Task<BaseResponseDto> UpdateRoleAsync(RoleUpdateRequestDto roleUpdateRequest);

        Task<BaseResponseDto> DeleteRoleAsync(string roleName);

        Task<BaseResponseDto> AssignRoleAsync(RoleAssignRequestDto roleAssignRequest);

        Task<BaseResponseDto> RevokeRoleAsync(RoleAssignRequestDto roleRevokeRequest);

        Task<BaseResponseDto> RoleChangeRequestAsync(string requestedRole);

        Task<BaseResponseDto> ReviewRoleChangeRequest(RoleChangeReviewDto roleChangeRequest);

        Task<IEnumerable<RoleChangeRequestDto>> GetAllRoleChangeRequest();
    }
}