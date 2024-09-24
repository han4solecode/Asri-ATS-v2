using AsriATS.Application.DTOs.JobPostRequest;
using AsriATS.Application.DTOs;

namespace AsriATS.Application.Contracts
{
    public interface IJobPostTemplateRequestService
    {
        Task<BaseResponseDto> SubmitJobTemplateRequest(JobPostRequestDto request);
    }
}
