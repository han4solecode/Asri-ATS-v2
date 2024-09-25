using System.ComponentModel.DataAnnotations;

namespace AsriATS.Domain.Entities
{
    public class Company
    {
        public int CompanyId { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Address { get; set; } = null!;

        // collection navigation to AppUser
        public virtual ICollection<AppUser> AppUsers { get; set; } = [];
        public virtual ICollection<RecruiterRegistrationRequest> RecruiterRegistrationRequests { get; set; } = [];

        // collection navigation to JobPostRequest
        public virtual ICollection<JobPostRequest> JobPostRequests { get; set; } = [];

        // collection navigation to JobPostTemplateRequest
        public virtual ICollection<JobPostTemplateRequest> JobPostTemplateRequests { get; set; } = [];

        // collection navigation to JobPostTemplate
        public virtual ICollection<JobPostTemplate> JobPostTemplates { get; set; } = [];
    }
}