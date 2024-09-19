using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.RecruiterRegistrationRequest
{
    public class RecruiterRegistrationRequestDto
    {
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Address { get; set; } = null!;
        public int CompanyId { get; set; }
    }
}
