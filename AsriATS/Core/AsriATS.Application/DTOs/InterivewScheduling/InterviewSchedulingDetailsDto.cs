using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.InterivewScheduling
{
    public class InterviewSchedulingDetailsDto
    {
        public int InterviewSchedulingId { get; set; }
        public int ApplicationId { get; set; }
        public DateTime InterviewTime { get; set; }
        public List<string> Interviewer { get; set; } = new List<string>();
        public List<string> InterviewersComments { get; set; } = new List<string>();
        public string InterviewType { get; set; } = null!;
        public string Location { get; set; } = null!;
        public bool? IsConfirmed { get; set; } = null;
        public int ProcessId { get; set; }
    }
}
