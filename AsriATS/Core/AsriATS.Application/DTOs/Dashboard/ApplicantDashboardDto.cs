using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Dashboard
{
    public class ApplicantDashboardDto
    {
        public object? ApplicationPipeline { get; set; }
        public IEnumerable<object>? InterviewSchedule { get; set; }
    }
}
