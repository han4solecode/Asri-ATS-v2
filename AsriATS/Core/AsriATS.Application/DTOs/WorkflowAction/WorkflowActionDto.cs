using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.WorkflowAction
{
    public class WorkflowActionDto
    {
        public DateTime? ActionDate { get; set; }
        public string ActionBy { get; set; }
        public string Action { get; set; }
        public string Comments { get; set; }
    }
}
