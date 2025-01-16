
namespace AsriATS.Application.DTOs.JobPostTemplateRequest
{
    public class JobPostTemplateRequestResponseDto : BaseResponseDto
    {
        public int? JobTemplateRequestId { get; set; }
        public int? JobTemplateId { get; set; }
        public string? JobTitle { get; set; }

        public string? CompanyName { get; set; }

        public string? Description { get; set; }

        public string? Requirements { get; set; }

        public string? Location { get; set; }

        public decimal? MinSalary { get; set; }

        public decimal? MaxSalary { get; set; }

        public string? EmploymentType { get; set; }
        public bool? IsApproved { get; set; }

    }
}
