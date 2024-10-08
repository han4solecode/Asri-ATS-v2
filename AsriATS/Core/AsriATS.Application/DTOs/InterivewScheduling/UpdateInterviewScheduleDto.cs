namespace AsriATS.Application.DTOs.InterivewScheduling
{
    public class UpdateInterviewScheduleDto
    {
        public int ProcessId { get; set; }

        public DateTime InterviewTime { get; set; }

        public string Comment { get; set; } = null!;
    }
}