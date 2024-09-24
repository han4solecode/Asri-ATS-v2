using AsriATS.Application.DTOs.JobPostRequest;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.JobPostTemplateRequest;

namespace AsriATS.Application.Contracts
{
    public interface IJobPostTemplateRequestService
    {
        Task<BaseResponseDto> SubmitJobTemplateRequest(JobPostTemplateRequestDto request);
    }
}
