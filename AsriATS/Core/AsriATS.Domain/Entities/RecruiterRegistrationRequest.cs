using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Domain.Entities
{
    public class RecruiterRegistrationRequest
    {
        [Key]
        public int RecruiterRegistrationRequestId { get; set; }
        public string ProcessName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public DateTime StartDate { get; set; }
        public DateTime? EndDate { get; set; } = null;

        // reference to Process
        public int ProcessId { get; set; }
        public virtual Process ProcessIdNavigation { get; set; } = null!;
    }
}
