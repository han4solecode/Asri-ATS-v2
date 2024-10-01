using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.ApplicationJob
{
    public class SupportingDocumentDto
    {
        public string? DocumentName { get; set; }
        public string? FilePath { get; set; }
        public DateTime UploadedDate { get; set; }
    }
}
