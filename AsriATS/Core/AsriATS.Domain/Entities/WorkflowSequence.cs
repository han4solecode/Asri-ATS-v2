using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace AsriATS.Domain.Entities
{
    public class WorkflowSequence
    {
        [Key]
        public int StepId { get; set; }
        [ForeignKey("Workflow")]
        public int WorkflowId { get; set; }
        public virtual Workflow Workflow { get; set; }
        public int StepOrder { get; set; }
        public string StepName { get; set; }
        [ForeignKey("Role")]
        public string? RequiredRole { get; set; }
        public virtual AppRole Role { get; set; }
    }
}
