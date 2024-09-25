using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;

namespace AsriATS.Application.Persistance
{
    public interface IJobPostRepository : IBaseRepository<JobPost>
    {
        IQueryable<JobPost> SearchJobPostAsync();
    }
}
