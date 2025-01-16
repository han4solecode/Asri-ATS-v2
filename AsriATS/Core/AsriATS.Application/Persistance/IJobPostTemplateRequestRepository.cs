using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.JobPost;
using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;
using System.Linq.Expressions;

namespace AsriATS.Application.Persistance
{
    public interface IJobTemplateRequestRepository : IBaseRepository<JobPostTemplateRequest>
    {
        Task<(IEnumerable<JobPostTemplateRequest>, int totalCount)> GetAllJobPostTemplateRequest(Expression<Func<JobPostTemplateRequest, bool>>? expression, JobPostSearch? queryObject, Pagination? pagination);

    }
}
