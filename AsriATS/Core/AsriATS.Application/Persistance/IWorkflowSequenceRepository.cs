using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.Persistance
{
    public interface IWorkflowSequenceRepository : IBaseRepository<WorkflowSequence>
    {
        Task<WorkflowSequence?> GetFirstOrDefaultAsync(Expression<Func<WorkflowSequence, bool>> expression);
        Task<IEnumerable<WorkflowSequence>> GetAllAsync(Expression<Func<WorkflowSequence, bool>> expression);
    }
}
