

namespace AsriATS.Application.DTOs.InterivewScheduling
{
    public class InterviewSchedulingRequestDto
    {
        public int ApplicantId { get; set; }
        public string InterviewTime { get; set; } = null!;
        public List<string> Interviewers { get; set; } = new List<string>();
        public List<string> InterviewerEmails { get; set; } = new List<string>();
        public string InterviewType { get; set; } = null!;
        public string Action { get; set; } = null!;
        public string Comment { get; set; } = null!;
    }
}
