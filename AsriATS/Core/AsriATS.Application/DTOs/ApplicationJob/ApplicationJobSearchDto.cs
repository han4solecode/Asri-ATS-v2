using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.ApplicationJob
{
    public class ApplicationJobSearchDto
    {
        public int? ApplicationJobId { get; set; }
        public string? JobTitle { get; set; }
        public string? ApplicantName { get; set; }
        public string? Status { get; set; }
        public string? Keywords { get; set; }
        public string? SortBy { get; set; } = null;
        public string? SortOrder { get; set; } = "asc";
        public int? PageNumber { get; set; } = 1;
        public int? PageSize { get; set; } = 20;
        public string? JobPostRequestStatus { get; set; }
    }
}
