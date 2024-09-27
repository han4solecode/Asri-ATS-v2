using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;
using System.Linq.Expressions;

namespace AsriATS.Application.Persistance
{
    public interface IJobPostTemplateRepository : IBaseRepository<JobPostTemplate>
    {
        Task<IEnumerable<JobPostTemplate>> GetAllAsync(Expression<Func<JobPostTemplate, bool>> expression);
    }
}