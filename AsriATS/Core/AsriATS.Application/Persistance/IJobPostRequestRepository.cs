using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.JobPost;
using AsriATS.Application.DTOs.Report;
using AsriATS.Application.Persistance.Common;
using AsriATS.Domain.Entities;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace AsriATS.Application.Persistance
{
    public interface IJobPostRequestRepository : IBaseRepository<JobPostRequest>
    {
        Task<(IEnumerable<JobPostRequest>, int totalCount)> GetAllToBeReviewedAsync(int companyId, string userRoleId, JobPostSearch queryObject, Pagination pagination);
        Task<JobPostRequest?> GetFirstOrDefaultAsync(Expression<Func<JobPostRequest, bool>> expression);
        Task<List<ComplianceApprovalMetricsDto>> GetJobPostApprovalMetricsByCompanyAsync();
        Task<(IEnumerable<JobPostRequest>, int totalCount)> GetAllJobPostRequestsForRecruiter(string userId, JobPostSearch queryObject, Pagination pagination);
        Task<(IEnumerable<JobPostRequest>, int totalCount)> GetAllJobPostRequestHistoryHRManager(int? companyId, JobPostSearch queryObject, Pagination pagination);
    }
}
