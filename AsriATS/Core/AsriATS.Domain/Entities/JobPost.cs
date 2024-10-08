using System.ComponentModel.DataAnnotations;

namespace AsriATS.Domain.Entities
{
    public class JobPost
    {
        [Key]
        public int JobPostId { get; set; }

        public string JobTitle { get; set; } = null!;

        // refenrece to Company
        public int CompanyId { get; set; }
        public virtual Company CompanyIdNavigation { get; set; } = null!;

        public string Description { get; set; } = null!;

        public string Requirements { get; set; } = null!;

        public string Location { get; set; } = null!;

        [Range(0, double.PositiveInfinity, ErrorMessage = "Amount must be greater than 0.")]
        public decimal MinSalary { get; set; }

        [Range(0, double.PositiveInfinity, ErrorMessage = "Amount must be greater than 0.")]
        public decimal MaxSalary { get; set; }

        public string EmploymentType { get; set; } = null!;
        public DateTime CreatedDate { get; set; }

        public virtual ICollection<ApplicationJob> ApplicationJobsNavigation { get; set; } = []!;
    }
}