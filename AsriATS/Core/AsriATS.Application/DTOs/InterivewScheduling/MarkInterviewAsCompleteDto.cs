namespace AsriATS.Application.DTOs.InterivewScheduling
{
    public class MarkInterviewAsCompleteDto
    {
        public int ProcessId { get; set; }

        public List<string> InterviewersComments { get; set; } = [];

        public string Comment { get; set; } = null!;
    }
}