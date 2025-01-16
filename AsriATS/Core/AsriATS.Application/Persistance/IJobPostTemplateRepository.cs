using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.JobPost;
using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;
using System.Linq.Expressions;

namespace AsriATS.Application.Persistance
{
    public interface IJobPostTemplateRepository : IBaseRepository<JobPostTemplate>
    {
        Task<(IEnumerable<JobPostTemplate>, int totalCount)> GetAllAsync(Expression<Func<JobPostTemplate, bool>> expression, JobPostSearch? queryObject, Pagination? pagination);
    }
}