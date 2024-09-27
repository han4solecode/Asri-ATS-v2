

using AsriATS.Application.DTOs.JobPostTemplateRequest;
using AsriATS.Application.DTOs;
using AsriATS.Domain.Entities;

namespace AsriATS.Application.Contracts
{
    public interface IJobPostTemplateService
    {
        Task<IEnumerable<object>> GetAllJobPostTemplate();

        Task<JobPostTemplateRequestResponseDto> GetJobPostTemplate(int id);
    }
}
