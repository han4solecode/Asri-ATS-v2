using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;

namespace AsriATS.Application.Persistance
{
    public interface IWorkflowRepository : IBaseRepository<Workflow>
    {
        Task<Workflow?> GetWorkflowByNameAsync(string workflowName);
    }
}
