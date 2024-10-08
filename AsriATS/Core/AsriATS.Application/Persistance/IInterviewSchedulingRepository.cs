using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;
using System.Linq.Expressions;

namespace AsriATS.Application.Persistance
{
    public interface IInterviewSchedulingRepository : IBaseRepository<InterviewScheduling>
    {
        Task<IEnumerable<InterviewScheduling>> GetAllInterviewSchedulesAsync(Expression<Func<InterviewScheduling, bool>> expression);
    }
}
