
namespace AsriATS.Application.DTOs.JobPostRequest
{
    public class JobPostRequestResponseDto : BaseResponseDto
    {
        public int ProcessId { get; set; }
        public string? Requester { get; set; }
        public string? RequiredRole { get; set; }
        public DateTime? RequestDate { get; set; }
        public string? JobTitle { get; set; }

        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }

        public string? Description { get; set; }

        public string? Requirements { get; set; }

        public string? Location { get; set; }

        public decimal? MinSalary { get; set; }

        public decimal? MaxSalary { get; set; }

        public string? EmploymentType { get; set; }
        public string? CurrentStatus { get; set; }
        public string? Comments { get; set; }

        public object? RequestHistory { get; set; }

    }
}
