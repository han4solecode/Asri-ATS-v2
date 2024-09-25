using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;

namespace AsriATS.Application.Persistance
{
    public interface IJobTemplateRequestRepository : IBaseRepository<JobPostTemplateRequest>
    {
        Task<IEnumerable<JobPostTemplateRequest>> GetAllToBeReviewed();

    }
}
