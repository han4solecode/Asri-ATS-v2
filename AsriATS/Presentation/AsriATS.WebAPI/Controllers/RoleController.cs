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

        [Authorize(Roles = "Administrator")]
        [HttpPost("delete")]
        public async Task<IActionResult> DeleteRole([FromBody] string roleName)
        {
            var res = await _roleService.DeleteRoleAsync(roleName);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

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
    }
}