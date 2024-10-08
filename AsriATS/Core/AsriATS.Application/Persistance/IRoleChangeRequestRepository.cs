using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;

namespace AsriATS.Application.Persistance
{
    public interface IRoleChangeRequestRepository : IBaseRepository<RoleChangeRequest>
    {
        Task<IEnumerable<RoleChangeRequest>> GetAllToBeReviewedAsync();
    }
}