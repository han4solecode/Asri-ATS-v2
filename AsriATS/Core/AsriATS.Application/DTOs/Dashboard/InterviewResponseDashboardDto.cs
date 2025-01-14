using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Dashboard
{
    public class InterviewResponseDashboardDto
    {
        public IEnumerable<object>? Data { get; set; }
        public int TotalPages { get; set; }
    }
}
