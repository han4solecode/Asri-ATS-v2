using AsriATS.Application.DTOs.JobPostRequest;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.JobPostTemplateRequest;
using AsriATS.Domain.Entities;

namespace AsriATS.Application.Contracts
{
    public interface IJobPostTemplateRequestService
    {
        Task<BaseResponseDto> SubmitJobTemplateRequest(JobPostTemplateRequestDto request);

        Task<IEnumerable<JobPostTemplateRequest>> GetAllJobPostTemplateRequestToReview();
    }
}
