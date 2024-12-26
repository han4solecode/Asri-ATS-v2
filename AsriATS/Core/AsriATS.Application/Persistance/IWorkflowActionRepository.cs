using AsriATS.Application.DTOs.WorkflowAction;
using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;
using System.Linq.Expressions;

namespace AsriATS.Application.Persistance
{
    public interface IWorkflowActionRepository : IBaseRepository<WorkflowAction>
    {
        Task<WorkflowAction?> GetFirstOrDefaultAsync(Expression<Func<WorkflowAction, bool>> expression);
        Task<IEnumerable<RecruitmentFunnelDto>> GetRecruitmentFlunnel(int? companyId);
        Task<IEnumerable<WorkflowAction>> GetAllAsync(Expression<Func<WorkflowAction, bool>> expression);
    }
}
