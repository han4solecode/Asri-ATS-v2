using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace AsriATS.Domain.Entities
{
    public class AppUser : IdentityUser
    {
        [Required]
        public string FirstName { get; set; }

        [Required]
        public string LastName { get; set; }

        [Required]
        public string Address { get; set; }

        [Required]
        public DateOnly Dob { get; set; }

        [Required]
        public string Sex { get; set; }

        // refresh token
        public string? RefreshToken { get; set; }
        public DateTime? RefreshTokenExpiryTime { get; set; }

        public virtual ICollection<WorkflowAction> WorkflowActions { get; set; } = new List<WorkflowAction>();
        public virtual ICollection<Process> Processes { get; set; } = new List<Process>();

        // reference to Company
        public int? CompanyId { get; set; }
        public virtual Company? CompanyIdNavigation { get; set; }

        // navigation to RoleChangeRequest
        public virtual ICollection<RoleChangeRequest> RoleChangeRequests { get; set; } = [];

        // navigation to JobPostTemplateRequest
        public virtual ICollection<JobPostTemplateRequest> JobPostTemplateRequests { get; set; } = [];
    }
}