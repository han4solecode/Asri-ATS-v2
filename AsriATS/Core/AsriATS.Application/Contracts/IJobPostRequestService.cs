using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.JobPostRequest;

namespace AsriATS.Application.Contracts
{
    public interface IJobPostRequestService
    {
        Task<BaseResponseDto> SubmitJobPostRequest(JobPostRequestDto request);
    }
}
