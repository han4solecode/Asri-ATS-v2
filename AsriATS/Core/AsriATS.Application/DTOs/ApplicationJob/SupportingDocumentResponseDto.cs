namespace AsriATS.Application.DTOs.ApplicationJob
{
    public class SupportingDocumentResponseDto : BaseResponseDto
    {
        public IEnumerable<object>? Document { get; set; }
    }
}