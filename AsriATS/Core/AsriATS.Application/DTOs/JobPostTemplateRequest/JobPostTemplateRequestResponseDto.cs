
namespace AsriATS.Application.DTOs.JobPostTemplateRequest
{
    public class JobPostTemplateRequestResponseDto : BaseResponseDto
    {
        public string JobTitle { get; set; } = null!;

        public int CompanyId { get; set; }

        public string Description { get; set; } = null!;

        public string Requirements { get; set; } = null!;

        public string Location { get; set; } = null!;

        public decimal MinSalary { get; set; }

        public decimal MaxSalary { get; set; }

        public string EmploymentType { get; set; } = null!;

    }
}
