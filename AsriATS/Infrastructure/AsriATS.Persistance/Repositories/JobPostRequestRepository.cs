using AsriATS.Application.DTOs.Report;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AsriATS.Persistance.Repositories
{
    public class JobPostRequestRepository : IJobPostRequestRepository
    {
        private readonly AppDbContext _context;

        public JobPostRequestRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task CreateAsync(JobPostRequest entity)
        {
            await _context.JobPostRequests.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(JobPostRequest entity)
        {
            _context.JobPostRequests.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<JobPostRequest>> GetAllAsync()
        {
            var jobPostRequests = await _context.JobPostRequests.ToListAsync();

            return jobPostRequests;
        }

        public async Task<IEnumerable<JobPostRequest>> GetAllToBeReviewedAsync(int companyId, string userRoleId)
        {
            var jobPostRequest = await _context.JobPostRequests.Include(r => r.ProcessIdNavigation).ThenInclude(p => p.WorkflowSequence).Include(r => r.ProcessIdNavigation).ThenInclude(p => p.Requester).Include(r => r.ProcessIdNavigation).ThenInclude(p => p.WorkflowActions).Where(r => r.CompanyId == companyId && r.ProcessIdNavigation.WorkflowSequence.RequiredRole == userRoleId).ToListAsync();

            return jobPostRequest;
        }

        public async Task<JobPostRequest?> GetByIdAsync(int id)
        {
            var jobPostRequest = await _context.JobPostRequests.FindAsync(id);

            return jobPostRequest;
        }

        public async Task UpdateAsync(JobPostRequest entity)
        {
            _context.JobPostRequests.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<JobPostRequest?> GetFirstOrDefaultAsync(Expression<Func<JobPostRequest, bool>> expression)
        {
            return await _context.JobPostRequests.Include(jpr => jpr.ProcessIdNavigation).ThenInclude(p =>p.Requester).FirstOrDefaultAsync(expression);
        }

        public async Task<List<ComplianceApprovalMetricsDto>> GetJobPostApprovalMetricsByCompanyAsync()
        {
            var jobRequests = await (from jpr in _context.JobPostRequests
                                     join p in _context.Processes on jpr.ProcessId equals p.ProcessId
                                     where p.Status == "Approved by HR Manager" || p.Status == "Rejected by HR Manager"
                                     select new
                                     {
                                         jpr.JobPostRequestId,
                                         jpr.CompanyId,
                                         RequestDate = p.RequestDate,
                                         Status = p.Status
                                     })
                            .ToListAsync();

            var approvedJobRequests = jobRequests.Where(r => r.Status == "Approved by HR Manager").ToList();
            var approvedJobPostIds = approvedJobRequests.Select(req => req.JobPostRequestId).ToList();

            var approvedJobPosts = await _context.JobPosts
                                                 .Where(jp => approvedJobPostIds.Contains(jp.JobPostId)) // Assuming JobPostId is the key
                                                 .ToListAsync();

            var jobPostApprovalMetrics = jobRequests
                .GroupBy(req => req.CompanyId)
                .Select(g =>
                {
                    var companyApprovedPosts = approvedJobPosts.Where(jp => g.Select(req => req.JobPostRequestId).Contains(jp.JobPostId)).ToList();
                    var totalApproved = companyApprovedPosts.Count;
                    var approvalTimes = companyApprovedPosts
                        .Select(jp =>
                        {
                            var request = g.FirstOrDefault(req => req.JobPostRequestId == jp.JobPostId);
                            return request != null ? (jp.CreatedDate - request.RequestDate).TotalDays : 0;
                        })
                        .ToList();

                    var averageApprovalTime = totalApproved > 0 ? approvalTimes.Average() : 0;

                    var totalRejected = g.Count(req => req.Status == "Rejected by HR Manager");

                    return new ComplianceApprovalMetricsDto
                    {
                        CompanyId = g.Key,
                        TotalApproved = totalApproved,
                        TotalRejected = totalRejected,
                        AverageApprovalTime = averageApprovalTime
                    };
                })
                .ToList();

            return jobPostApprovalMetrics;
        }
        public async Task<IEnumerable<JobPostRequest>> GetAllJobPostRequestsForRecruiter(string userId)
        {
            var jobPostRequest = await _context.JobPostRequests.Include(r => r.ProcessIdNavigation).ThenInclude(p => p.WorkflowSequence).Include(r => r.ProcessIdNavigation).ThenInclude(p => p.Requester).Include(r => r.ProcessIdNavigation).ThenInclude(p => p.WorkflowActions).Where(r => r.ProcessIdNavigation.RequesterId == userId ).ToListAsync();

            return jobPostRequest;
        }
    }
}
