using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Report
{
    public class JobPostStatusDto
    {
        public int JobPostId { get; set; }
        public DateTime CreatedDate { get; set; }
        public int CompanyId { get; set; }
        public string Status { get; set; }
    }
}
