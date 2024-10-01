using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Domain.Entities
{
    public class SupportingDocument
    {
        [Key]
        public int SupportingDocumentId { get; set; }

        // Navigation property for User
        [ForeignKey("AspNetUsers")]
        public string UserId { get; set; }
        public virtual AppUser UserIdNavigation { get; set; }

        public string? DocumentName { get; set; } // Name of the file
        public string? FilePath { get; set; } // Path where the file is stored
        public DateTime UploadedDate { get; set; }

        // Navigation property
        public virtual ICollection<ApplicationJob> ApplicationJobNavigation { get; set; } = null!;
    }
}
