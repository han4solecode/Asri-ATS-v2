using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.JobPostTemplateRequest
{
    public class JobPostTemplateRequestDto
    {
        public string JobTitle { get; set; } = null!;

        public int CompanyId { get; set; }

        public string Description { get; set; } = null!;

        public string Requirements { get; set; } = null!;

        public string Location { get; set; } = null!;

        [Range(0, double.PositiveInfinity, ErrorMessage = "Amount must be greater than 0.")]
        public decimal MinSalary { get; set; }

        [Range(0, double.PositiveInfinity, ErrorMessage = "Amount must be greater than 0.")]
        public decimal MaxSalary { get; set; }

        public string EmploymentType { get; set; } = null!;

    }
}
