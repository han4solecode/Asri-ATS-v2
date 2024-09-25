using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;

namespace AsriATS.Application.Persistance
{
    public interface IJobPostRequestRepository : IBaseRepository<JobPostRequest>
    {
        Task<IEnumerable<JobPostRequest>> GetAllToBeReviewedAsync(int companyId, string userRoleId);
    }
}
