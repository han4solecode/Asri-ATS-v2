using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.Persistance
{
    public interface IApplicationJobRepository:IBaseRepository<ApplicationJob>
    {
        Task<IEnumerable<ApplicationJob>> GetAllToStatusAsync(int companyId, string userRole);
        Task<IEnumerable<ApplicationJob>> GetAllByApplicantAsync(Expression<Func<ApplicationJob, bool>> expression);
        Task<ApplicationJob?> GetFirstOrDefaultAsync(Expression<Func<ApplicationJob, bool>> predicate);
        Task<ApplicationJob> GetFirstOrDefaultAsyncUpdate(Expression<Func<ApplicationJob, bool>> predicate, Func<IQueryable<ApplicationJob>, IIncludableQueryable<ApplicationJob, object>> include = null);
    }
}
