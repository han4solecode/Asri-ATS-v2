namespace AsriATS.Application.DTOs.Request
{
    public class ReviewRequestDto
    {
        public int ProcessId { get; set; }

        public string Action { get; set; } = null!;

        public string Comment { get; set; } = null!;
    }
}