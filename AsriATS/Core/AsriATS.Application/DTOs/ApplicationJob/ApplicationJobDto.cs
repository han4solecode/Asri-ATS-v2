using AsriATS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.ApplicationJob
{
    public class ApplicationJobDto
    {
        public string? WorkExperience { get; set; }
        public string? Education { get; set; }
        public string? Skills { get; set; }
        public int JobPostId { get; set; }
        public int SupportingDocumentsId { get; set; }
    }
}
