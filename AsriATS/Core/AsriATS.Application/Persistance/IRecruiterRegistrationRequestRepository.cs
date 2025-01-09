using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;
using System.Linq.Expressions;

namespace AsriATS.Application.Persistance
{
    public interface IRecruiterRegistrationRequestRepository : IBaseRepository<RecruiterRegistrationRequest>
    {
        Task<IEnumerable<RecruiterRegistrationRequest>> GetAllToBeReviewedAsync(Expression<Func<RecruiterRegistrationRequest, bool>>? filter = null);
        Task<RecruiterRegistrationRequest?> FindByEmailAsync(string email);
        IQueryable<RecruiterRegistrationRequest> GetAll();
    }
}
