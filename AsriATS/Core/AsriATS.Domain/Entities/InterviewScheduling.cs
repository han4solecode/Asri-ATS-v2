using Microsoft.EntityFrameworkCore.Metadata.Internal;
using System.ComponentModel.DataAnnotations.Schema;

namespace AsriATS.Domain.Entities
{
    public class InterviewScheduling
    {
        public int InterviewSchedulingId { get; set; }

        [ForeignKey("ApplicationJob")]
        public int ApplicationId { get; set; }
        public virtual ApplicationJob ApplicationIdNavigation { get; set; } = null!;
        public DateTime InterviewTime { get; set; }
        public List<string> Interviewer {  get; set; } = new List<string>();
        public List<string> InterviewersComments { get; set; } = new List<string>();
        public string InterviewType { get; set; } = null!;
        public string Location { get; set; } = null!;
    }
}
