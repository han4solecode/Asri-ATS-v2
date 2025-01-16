using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Role;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly IRoleService _roleService;

        public RoleController(IRoleService roleService)
        {
            _roleService = roleService;
        }

        // [Authorize(Roles = "Administrator")]
        /// <summary>
        /// Create a new role
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/role/create
        ///     {
        ///         "Applicant"
        ///     }
        /// 
        /// </remarks>
        /// <param name="roleName"></param>
        /// <returns></returns>
        [HttpPost("create")]
        public async Task<IActionResult> CreateRole([FromBody] string roleName)
        {
            var res = await _roleService.CreateRoleAsyc(roleName);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

        /// <summary>
        /// Update an existing role 
        /// </summary>
        /// <remarks>
        /// Only user with the role "Administrator" is authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/role/update
        ///     {
        ///         "RoleName": "Applicant",
        ///         "NewRoleName": "Job Seeker"
        ///     }
        /// 
        /// </remarks>
        /// <param name="roleUpdateRequest"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost("update")]
        public async Task<IActionResult> UpdateRole([FromBody] RoleUpdateRequestDto roleUpdateRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _roleService.UpdateRoleAsync(roleUpdateRequest);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

        /// <summary>
        /// Delete an existing role
        /// </summary>
        /// <remarks>
        /// Only user with the role "Administrator" is authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     DELETE /api/role/delete
        ///     {
        ///         "roleName"
        ///     }
        /// 
        /// </remarks>
        /// <param name="roleName"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpDelete("delete")]
        public async Task<IActionResult> DeleteRole([FromBody] string roleName)
        {
            var res = await _roleService.DeleteRoleAsync(roleName);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

        /// <summary>
        /// Assign a role to a specific user
        /// </summary>
        /// <remarks>
        /// Only user with the role "Administrator" is authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/role/assign
        ///     {
        ///         "AppuserId": "random-userid-guid",
        ///         "RoleName": "HR Manager"
        ///     }
        /// 
        /// </remarks>
        /// <param name="roleAssignRequest"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost("assign")]
        public async Task<IActionResult> AssignRole([FromBody] RoleAssignRequestDto roleAssignRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _roleService.AssignRoleAsync(roleAssignRequest);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

        /// <summary>
        /// Revoke a role from a specific user
        /// </summary>
        /// <remarks>
        /// Only user with the role "Administrator" is authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/role/revoke
        ///     {
        ///         "AppUserId": "random-userid-guid",
        ///         "RoleName": "Recruiter"
        ///     }
        /// 
        /// </remarks>
        /// <param name="roleRevokeRequest"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost("revoke")]
        public async Task<IActionResult> RevokeRole([FromBody] RoleAssignRequestDto roleRevokeRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _roleService.RevokeRoleAsync(roleRevokeRequest);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

        /// <summary>
        /// Create a role change request
        /// </summary>
        /// <remarks>
        /// Only user with the roles "Recruiter" and "HR Manager" are authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     POST /api/role/change
        ///     {
        ///         "HR Manager"
        ///     }
        /// 
        /// </remarks>
        /// <param name="requestedRole"></param>
        /// <returns></returns>
        [Authorize(Roles = "Recruiter, HR Manager")]
        [HttpPost("change")]
        public async Task<IActionResult> RoleChangeRequest([FromBody] string requestedRole)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _roleService.RoleChangeRequestAsync(requestedRole);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

        /// <summary>
        /// Review (approve or reject) a role change request
        /// </summary>
        /// <remarks>
        /// Only user with the role "Administrator" is authorized to access this endpoint.
        /// Action must be either "Approved" or "Rejected".
        /// 
        /// Sample request:
        /// 
        ///     POST /api/role/review-request
        ///     {
        ///         "RoleChangeRequestId": 1,
        ///         "Action": "Approved"
        ///     }
        /// 
        /// </remarks>
        /// <param name="roleChangeReview"></param>
        /// <returns></returns>
        [Authorize(Roles = "Administrator")]
        [HttpPost("review-request")]
        public async Task<IActionResult> ReviewRoleChangeRequest([FromBody] RoleChangeReviewDto roleChangeReview)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _roleService.ReviewRoleChangeRequest(roleChangeReview);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

        [Authorize(Roles = "Administrator")]
        [HttpGet("change-requests")]
        public async Task<IActionResult> GetAllRoleChangeRequests()
        {
            var res = await _roleService.GetAllRoleChangeRequest();

            return Ok(res);
        }
    }
}