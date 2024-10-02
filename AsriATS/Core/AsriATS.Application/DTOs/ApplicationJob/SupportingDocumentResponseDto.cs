namespace AsriATS.Application.DTOs.ApplicationJob
{
    public class SupportingDocumentResponseDto : BaseResponseDto
    {
        public IEnumerable<object>? Documents { get; set; }
    }
}