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
    public interface IDocumentSupportRepository:IBaseRepository<SupportingDocument>
    {
        Task<IEnumerable<SupportingDocument>> GetAllAsync(Expression<Func<SupportingDocument, bool>> expression);
        Task<IEnumerable<SupportingDocument>> GetByApplicationJobIdAsync(int applicationJobId);
    }
}
