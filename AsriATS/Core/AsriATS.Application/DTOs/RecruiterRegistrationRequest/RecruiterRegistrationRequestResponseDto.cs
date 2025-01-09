using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.RecruiterRegistrationRequest
{
    public class RecruiterRegistrationRequestResponseDto : BaseResponseDto
    {
        public string? FirstName { get; set; }
        public string? LastName { get; set; }
        public DateOnly? Dob { get; set; }
        public string? Sex { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Email { get; set; }
        public string? Address { get; set; }
        public string? CompanyName { get; set; }
        public bool? IsApproved { get; set; } = null;
    }
}
