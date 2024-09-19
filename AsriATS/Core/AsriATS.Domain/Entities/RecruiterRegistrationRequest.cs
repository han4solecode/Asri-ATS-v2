using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Domain.Entities
{
    public class RecruiterRegistrationRequest
    {
        [Key]
        public int RecruiterRegistrationRequestId { get; set; }
        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string Name { get; set; } = null!;
        [Required]
        public string Address { get; set; } = null!;
        [ForeignKey("Company")]
        public int CompanyId { get; set; }
        public virtual Company CompanyIdNavigation { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } = null;
        public bool? IsApproved { get; set; } = null;
    }
}
