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
        public string Name { get; set; } = null!;

        [Required]
        public string Address { get; set; } = null!;

        // reference to Process
        // public int ProcessId { get; set; }
        // public virtual Process ProcessIdNavigation { get; set; } = null!;
    }
}