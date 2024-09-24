using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;

namespace AsriATS.Application.Persistance
{
    public interface ICompanyRepository : IBaseRepository<Company>
    {
        IQueryable<Company> SearchCompanyAsync();
    }
}