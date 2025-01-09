using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.JobPost;
using AsriATS.Application.DTOs.JobPostRequest;
using AsriATS.Application.DTOs.Request;

namespace AsriATS.Application.Contracts
{
    public interface IJobPostRequestService
    {
        Task<BaseResponseDto> SubmitJobPostRequest(JobPostRequestDto request);

        Task<BaseResponseDto> ReviewJobPostRequest(ReviewRequestDto reviewRequest);

        Task<object> GetJobPostRequestToReview(JobPostSearch queryObject, Pagination pagination);

        Task<BaseResponseDto> UpdateJobPostRequest(UpdateJobPostRequestDto requestDto);

        Task<JobPostRequestResponseDto> GetJobPostRequest(int id);
        Task<object> GetJobPostRequestForRecruiter(JobPostSearch queryObject, Pagination pagination);
        Task<object> GetHistoryJobPostRequest(JobPostSearch queryObject, Pagination pagination);
    }
}
