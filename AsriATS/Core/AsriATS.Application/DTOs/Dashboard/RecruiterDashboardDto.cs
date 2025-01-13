using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Dashboard
{
    public class RecruiterDashboardDto
    {
        public object? ApplicationPipeline {  get; set; }
        public object? AnalyticSnapshot { get; set; }
        public object? TaskReminders { get; set; }
    }
}
