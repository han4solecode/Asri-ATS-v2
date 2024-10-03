using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.ApplicationJob
{
    public class UpdateApplicationJobDto
    {
        public int ApplicationJobId { get; set; } // Application job to be updated
        public int ProcessId { get; set; } // Process related to the job application
        public string WorkExperience { get; set; } // Updated work experience
        public string Education { get; set; } // Updated education details
        public string Skills { get; set; } // Updated skills

        // Comments or additional info provided by the applicant
        public string Comments { get; set; }

        // List of supporting documents to be updated or added
        public List<SupportingDocumentDto?> SupportingDocuments { get; set; } = new();
    }
}
