using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Report
{
    public class ApplicationJobSummaryDto
    {
        public int TotalApplications { get; set; }
        public List<ApplicationJobStatusDto> ApplicationSummaries { get; set; }
        public int TotalJobPosted { get; set; }
        public List<JobPostStatusDto> JobStatusDtos { get; set; }
    }
}
