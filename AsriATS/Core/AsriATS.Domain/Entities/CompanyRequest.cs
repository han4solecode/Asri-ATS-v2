using System.ComponentModel.DataAnnotations;

namespace AsriATS.Domain.Entities
{
    public class CompanyRequest
    {
        [Key]
        public int CompanyRequestId { get; set; }

        [Required]
        public string Email { get; set; } = null!;

        [Required]
        public string CompanyName { get; set; } = null!;

        [Required]
        public string CompanyAddress { get; set; } = null!;

        [Required]
        public string FirstName { get; set; } = null!;

        [Required]
        public string LastName { get; set; } = null!;

        [Required]
        public string UserAddress { get; set; } = null!;

        [Required]
        public DateOnly Dob { get; set; }

        [Required]
        public string Sex { get; set; } = null!;

        public bool? IsApproved { get; set; }

        // reference to Process
        // public int ProcessId { get; set; }
        // public virtual Process ProcessIdNavigation { get; set; } = null!;
    }
}