using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;

namespace AsriATS.Application.Persistance
{
    public interface IRecruiterRegistrationRequestRepository : IBaseRepository<RecruiterRegistrationRequest>
    {
        Task<IEnumerable<RecruiterRegistrationRequest>> GetAllToBeReviewedAsync();
        Task<RecruiterRegistrationRequest?> FindByEmailAsync(string email);
    }
}
