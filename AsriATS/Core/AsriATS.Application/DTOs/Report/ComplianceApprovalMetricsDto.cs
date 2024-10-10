using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Report
{
    public class ComplianceApprovalMetricsDto
    {
        public int CompanyId { get; set; }
        public int TotalApproved { get; set; }
        public int TotalRejected { get; set; }
        public double AverageApprovalTime { get; set; }
    }
}
