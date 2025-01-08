using AsriATS.Application.DTOs.InterivewScheduling;
using AsriATS.Application.DTOs.WorkflowAction;
using AsriATS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.DTOs.ApplicationJob
{
    public class ApplicationDetailDto
    {
        public int ApplicationJobId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? PhoneNumber { get; set; }
        public string? Address { get; set; }
        public string? WorkExperience { get; set; }
        public string? Education { get; set; }
        public string? Skills { get; set; }
        public DateTime UploadedDate { get; set; }
        public string? JobPostName { get; set; }
        public int ProcessId { get; set; }
        public string? Status { get; set; }
        public string? CurrentStep { get; set; }
        public string? RequiredRole {  get; set; }
        public List<WorkflowActionDto> WorkflowActions { get; set; }
        public List<SupportingDocumentDto> SupportingDocuments { get; set; }
        public List<InterviewSchedulingDetailsDto> InterviewSchedulingDetails { get; set; }
    }
}
