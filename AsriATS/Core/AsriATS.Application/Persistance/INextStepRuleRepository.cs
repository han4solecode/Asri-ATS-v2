using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;
using System.Linq.Expressions;

namespace AsriATS.Application.Persistance
{
    public interface INextStepRuleRepository : IBaseRepository<NextStepRule>
    {
        Task<NextStepRule?> GetFirstOrDefaultAsync(Expression<Func<NextStepRule, bool>> expression);
    }
}
