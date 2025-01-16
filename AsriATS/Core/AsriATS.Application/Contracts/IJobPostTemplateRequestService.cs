using AsriATS.Application.DTOs.JobPostRequest;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.JobPostTemplateRequest;
using AsriATS.Domain.Entities;
using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.JobPost;

namespace AsriATS.Application.Contracts
{
    public interface IJobPostTemplateRequestService
    {
        Task<BaseResponseDto> SubmitJobTemplateRequest(JobPostTemplateRequestDto request);
        Task<object> GetAllJobPostTemplateRequest(JobPostSearch? jobPostSearch, Pagination? pagination);

        Task<BaseResponseDto> ReviewJobPostTemplateRequest(JobPostTemplateReviewDto jobPostTemplateReview);

        Task<JobPostTemplateRequestResponseDto> GetJobPostTemplateRequest(int id);
    }
}
