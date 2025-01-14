using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;
using System.Linq.Expressions;

namespace AsriATS.Application.Persistance
{
    public interface IInterviewSchedulingRepository : IBaseRepository<InterviewScheduling>
    {
        Task<(IEnumerable<InterviewScheduling>, int totalCount)> GetAllInterviewSchedulesAsync(Expression<Func<InterviewScheduling, bool>> expression, Pagination? pagination);
        Task<List<InterviewScheduling>> GetByProcessIdAsync(int processId);
    }
}
