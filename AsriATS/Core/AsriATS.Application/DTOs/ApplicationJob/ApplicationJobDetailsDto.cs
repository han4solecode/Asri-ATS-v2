using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.ApplicationJob
{
    public class ApplicationJobDetailsDto
    {
        public Guid ApplicationJobId { get; set; }
        public string Name { get; set; }
        public string Email { get; set; }
        public string PhoneNumber { get; set; }
        public string Address { get; set; }
        public string WorkExperience { get; set; }
        public string Education { get; set; }
        public string Skills { get; set; }
        public Guid JobPostId { get; set; }
        public List<SupportingDocumentDto> SupportingDocuments { get; set; }
    }
}
