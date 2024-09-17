using System.ComponentModel.DataAnnotations;
using System.Diagnostics;
using Microsoft.AspNetCore.Identity;

namespace AsriATS.Domain.Entities
{
    public class Workflow
    {
        [Key]
        public int WorkflowId { get; set; }
        public string WorkflowName { get; set; }
        public string Description { get; set; }
        public ICollection<Process> Processes { get; set; } = new List<Process>();
        public ICollection<WorkflowSequence> WorkflowSequences { get; set; } = new List<WorkflowSequence>();
    }
}