using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace AsriATS.Domain.Entities
{
    public class RecruiterRegistrationRequest
    {
        [Key]
        public int RecruiterRegistrationRequestId { get; set; }
        [Required]
        public string Email { get; set; } = null!;
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }
        [Required]
        public DateOnly Dob { get; set; }

        [Required]
        public string Sex { get; set; }
        [Required]
        public string PhoneNumber { get; set; }
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
