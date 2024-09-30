using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Domain.Entities
{
    public class ApplicationJob
    {
        [Key]
        public int ApplicationJobId { get; set; }   
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        // reference to AppUser
        public string UserId { get; set; }
        [ForeignKey("UserId")]
        public virtual AppUser UserIdNavigation { get; set; } 

        public string? WorkExperience {  get; set; }
        public string? Education { get; set; }
        public string? Skills { get; set; }
        public DateTime UploadedDate { get; set; }

        // Navigation property for Job Post
        public int JobPostId { get; set; }
        public virtual JobPost JobPostNavigation { get; set; } = null!;

        public virtual List<SupportingDocument?> SupportingDocumentsIdNavigation { get; set; } = null!;

        // foreign key to process
        public int ProcessId { get; set; }
        public virtual Process ProcessIdNavigation { get; set; } = null!;
    }
}
