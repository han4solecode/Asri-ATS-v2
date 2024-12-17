using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.JobPost
{
    public class JobPostDetail
    {
        public int? JobPostId { get; set; }
        public string? JobTitle { get; set; }
        public string? Location { get; set; }
        public string? Description { get; set; }
        public string? Requirement { get; set; }
        public string? EmploymentType { get; set; }
        public decimal MinSalary { get; set; }
        public decimal MaxSalary { get; set; }
        public string? CompanyName { get; set; }
    }
}
