using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.WorkflowSequence
{
    public class WorkflowSequenceRequestDto
    {
        [Required]
        public int WorkflowId { get; set; }
        [Required]
        public int StepOrder { get; set; }
        [Required]
        public string StepName { get; set; } = null!;
        public string? RequiredRole { get; set; }
    }
}
