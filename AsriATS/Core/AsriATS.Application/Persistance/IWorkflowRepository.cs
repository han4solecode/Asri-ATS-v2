﻿using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;
using System.Linq.Expressions;

namespace AsriATS.Application.Persistance
{
    public interface IWorkflowRepository : IBaseRepository<Workflow>
    {
        Task<Workflow?> GetFirstOrDefaultAsync(Expression<Func<Workflow, bool>> expression);
    }
}
