using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Dashboard
{
    public class HRDashboardDto
    {
        public object? JobPostRequests { get; set; }
        public IEnumerable<object>? RecruiterRequests { get; set; }
        public IEnumerable<object>? RecruitmentFunnel { get; set; }
    }
}
