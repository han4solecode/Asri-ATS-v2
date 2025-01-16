

using AsriATS.Application.DTOs.JobPostTemplateRequest;
using AsriATS.Application.DTOs;
using AsriATS.Domain.Entities;
using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.JobPost;

namespace AsriATS.Application.Contracts
{
    public interface IJobPostTemplateService
    {
        Task<object> GetAllJobPostTemplate(JobPostSearch jobPostSearch, Pagination pagination);

        Task<JobPostTemplateRequestResponseDto> GetJobPostTemplate(int id);
    }
}
