using System.ComponentModel.DataAnnotations;

namespace AsriATS.Domain.Entities
{
    public class Request
    {
        [Key]
        public int RequestId { get; set; }
        public string ProcessName { get; set; }
        public string Description { get; set; }
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } = null;
        public virtual Process Process { get; set; }
    }
}
