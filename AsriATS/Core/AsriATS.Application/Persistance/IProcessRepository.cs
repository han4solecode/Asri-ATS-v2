using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;
using System.Linq.Expressions;

namespace AsriATS.Application.Persistance
{
    public interface IProcessRepository : IBaseRepository<Process>
    {
        Task<Process?> GetFirstOrDefaultAsync(Expression<Func<Process, bool>> expression);
        Task<ApplicationJob> GetByProcessIdAsync(int processId);
    }
}
