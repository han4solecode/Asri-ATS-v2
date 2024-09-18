using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Update
{
    public class UpdateRequestDto
    {
        public string? Username { get; set; } // Could also be UserId if preferred
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Firstname { get; set; }
        public string? Lastname { get; set; }
        public string? Address { get; set; }
        public DateOnly Dob { get; set; }
        public string? Sex { get; set; }
    }
}
