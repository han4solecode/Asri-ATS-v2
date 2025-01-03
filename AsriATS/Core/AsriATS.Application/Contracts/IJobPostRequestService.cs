﻿using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.JobPostRequest;
using AsriATS.Application.DTOs.Request;

namespace AsriATS.Application.Contracts
{
    public interface IJobPostRequestService
    {
        Task<BaseResponseDto> SubmitJobPostRequest(JobPostRequestDto request);

        Task<BaseResponseDto> ReviewJobPostRequest(ReviewRequestDto reviewRequest);

        Task<IEnumerable<object>> GetJobPostRequestToReview();

        Task<BaseResponseDto> UpdateJobPostRequest(UpdateJobPostRequestDto requestDto);

        Task<JobPostRequestResponseDto> GetJobPostRequest(int id);
        Task<IEnumerable<object>> GetJobPostRequestForRecruiter();
    }
}
