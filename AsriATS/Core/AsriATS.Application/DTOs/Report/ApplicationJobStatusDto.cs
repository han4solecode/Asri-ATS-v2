using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.Report
{
    public class ApplicationJobStatusDto
    {
        public int ApplicationJobId { get; set; }
        public DateTime UploadedDate { get; set; }
        public string Status { get; set; }
    }
}
