using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Role
{
    public class RoleChangeRequestDto
    {
        public int RoleChangeRequestId { get; set; }
        public string? UserId { get; set; }
        public string? UserFullName { get; set; }
        public string? CurrentRole { get; set; }
        public string? RequestedRole { get; set; }
        public string? CompanyName { get; set; }
    }
}
