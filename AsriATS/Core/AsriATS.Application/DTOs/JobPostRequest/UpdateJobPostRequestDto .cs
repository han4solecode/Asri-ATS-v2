using System.ComponentModel.DataAnnotations;


namespace AsriATS.Application.DTOs.JobPostRequest
{
    public class UpdateJobPostRequestDto
    {
        public string JobTitle { get; set; } = null!;
        public int ProcessId { get; set; }

        public string Description { get; set; } = null!;

        public string Requirements { get; set; } = null!;

        public string Location { get; set; } = null!;

        [Range(0, double.PositiveInfinity, ErrorMessage = "Amount must be greater than 0.")]
        public decimal MinSalary { get; set; }

        [Range(0, double.PositiveInfinity, ErrorMessage = "Amount must be greater than 0.")]
        public decimal MaxSalary { get; set; }

        public string EmploymentType { get; set; } = null!;

        public string Comments { get; set; } = null!;
    }
}
