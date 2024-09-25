
namespace AsriATS.Application.DTOs.JobPostRequest
{
    public class JobPostRequestResponseDto : BaseResponseDto
    {
        public string? JobTitle { get; set; }

        public int? CompanyId { get; set; }

        public string? Description { get; set; }

        public string? Requirements { get; set; }

        public string? Location { get; set; }

        public decimal? MinSalary { get; set; }

        public decimal? MaxSalary { get; set; }

        public string? EmploymentType { get; set; }

    }
}
