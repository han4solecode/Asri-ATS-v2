using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.JobPost;
using AsriATS.Application.DTOs.Report;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Expressions;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

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

        public async Task<(IEnumerable<JobPostRequest>, int totalCount)> GetAllToBeReviewedAsync(int companyId, string userRoleId, JobPostSearch queryObject, Pagination pagination)
        {
            var query = _context.JobPostRequests.AsQueryable();
            var pageSize = pagination.PageSize ?? 10;  // Default page size to 10 if not provided
            var pageNumber = pagination.PageNumber ?? 1;  // Default page number to 1 if not provided

            // Ensure pageSize and pageNumber are valid
            pageSize = Math.Max(1, pageSize);  // Ensure page size is at least 1
            pageNumber = Math.Max(1, pageNumber);  // Ensure page number is at least 1

            var skipNumber = (pageNumber - 1) * pageSize;

            query = query.Include(r => r.ProcessIdNavigation).ThenInclude(p => p.Requester)
                         .Include(r => r.ProcessIdNavigation).ThenInclude(p => p.WorkflowActions);

            if (!string.IsNullOrEmpty(queryObject.Keywords))
            {
                var keyword = queryObject.Keywords.ToLower();
                int.TryParse(queryObject.Keywords, out int keywordAsInt); // Try to parse keyword as integer

                query = query.Where(j =>
                    (j.JobTitle != null && j.JobTitle.ToLower().Contains(keyword)) ||
                    (j.Description != null && j.Description.ToLower().Contains(keyword)) ||
                    (j.Requirements != null && j.Requirements.ToLower().Contains(keyword)) ||
                    (j.EmploymentType != null && j.EmploymentType.ToLower().Contains(keyword)) ||
                    (j.Location != null && j.Location.ToLower().Contains(keyword)) ||
                    (keywordAsInt > 0 && (j.MinSalary == keywordAsInt || j.MaxSalary == keywordAsInt)) // Match numeric salary
                );
            }

            if (queryObject != null)
            {
                if (!string.IsNullOrEmpty(queryObject.JobTitle))
                    query = query.Where(j => j.JobTitle.ToLower().Contains(queryObject.JobTitle.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.Location))
                    query = query.Where(j => j.Location.ToLower().Contains(queryObject.Location.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.Description))
                    query = query.Where(j => j.Description.ToLower().Contains(queryObject.Description.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.Requirement))
                    query = query.Where(j => j.Requirements.ToLower().Contains(queryObject.Requirement.ToLower()));

                if (queryObject.MinSalary != null && queryObject.MinSalary > 0)
                    query = query.Where(j => j.MinSalary >= queryObject.MinSalary);

                if (queryObject.MaxSalary != null && queryObject.MaxSalary > 0)
                    query = query.Where(j => j.MaxSalary <= queryObject.MaxSalary);

                if (!string.IsNullOrEmpty(queryObject.EmploymentType))
                    query = query.Where(j => j.EmploymentType.ToLower().Contains(queryObject.EmploymentType.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.JobPostRequestStatus))
                    query = query.Where(j => j.ProcessIdNavigation.Status == queryObject.JobPostRequestStatus);
            }

            query = query.Where(r => r.CompanyId == companyId && r.ProcessIdNavigation.WorkflowSequence.RequiredRole == userRoleId);

            query = query.OrderByDescending(j => j.ProcessIdNavigation.RequestDate);

            int totalCount = await query.CountAsync();

            query = query.Skip(skipNumber).Take(pageSize);

            var jobPostRequests = await query.ToListAsync();

            return (jobPostRequests, totalCount);
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
            return await _context.JobPostRequests.Include(jpr => jpr.ProcessIdNavigation).ThenInclude(p => p.WorkflowActions).Include(jpr => jpr.ProcessIdNavigation).ThenInclude(p =>p.Requester).Include(jpr => jpr.ProcessIdNavigation).ThenInclude(p => p.WorkflowSequence).ThenInclude(w=>w.Role).FirstOrDefaultAsync(expression);
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
        public async Task<(IEnumerable<JobPostRequest>,int totalCount)> GetAllJobPostRequestsForRecruiter(string userId, JobPostSearch queryObject, Pagination pagination)
        {
            var query = _context.JobPostRequests.AsQueryable();
            var pageSize = pagination.PageSize ?? 10;  // Default page size to 10 if not provided
            var pageNumber = pagination.PageNumber ?? 1;  // Default page number to 1 if not provided

            // Ensure pageSize and pageNumber are valid
            pageSize = Math.Max(1, pageSize);  // Ensure page size is at least 1
            pageNumber = Math.Max(1, pageNumber);  // Ensure page number is at least 1

            var skipNumber = (pageNumber - 1) * pageSize;

            query = query.Include(r => r.ProcessIdNavigation).ThenInclude(p => p.Requester)
                         .Include(r => r.ProcessIdNavigation).ThenInclude(p => p.WorkflowActions);

            if (!string.IsNullOrEmpty(queryObject.Keywords))
            {
                var keyword = queryObject.Keywords.ToLower();
                int.TryParse(queryObject.Keywords, out int keywordAsInt); // Try to parse keyword as integer

                query = query.Where(j =>
                    (j.JobTitle != null && j.JobTitle.ToLower().Contains(keyword)) ||
                    (j.Description != null && j.Description.ToLower().Contains(keyword)) ||
                    (j.Requirements != null && j.Requirements.ToLower().Contains(keyword)) ||
                    (j.EmploymentType != null && j.EmploymentType.ToLower().Contains(keyword)) ||
                    (j.Location != null && j.Location.ToLower().Contains(keyword)) ||
                    (keywordAsInt > 0 && (j.MinSalary == keywordAsInt || j.MaxSalary == keywordAsInt)) // Match numeric salary
                );
            }

            if (queryObject != null)
            {
                if (!string.IsNullOrEmpty(queryObject.JobTitle))
                    query = query.Where(j => j.JobTitle.ToLower().Contains(queryObject.JobTitle.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.Location))
                    query = query.Where(j => j.Location.ToLower().Contains(queryObject.Location.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.Description))
                    query = query.Where(j => j.Description.ToLower().Contains(queryObject.Description.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.Requirement))
                    query = query.Where(j => j.Requirements.ToLower().Contains(queryObject.Requirement.ToLower()));

                if (queryObject.MinSalary != null && queryObject.MinSalary > 0)
                    query = query.Where(j => j.MinSalary >= queryObject.MinSalary);

                if (queryObject.MaxSalary != null && queryObject.MaxSalary > 0)
                    query = query.Where(j => j.MaxSalary <= queryObject.MaxSalary);

                if (!string.IsNullOrEmpty(queryObject.EmploymentType))
                    query = query.Where(j => j.EmploymentType.ToLower().Contains(queryObject.EmploymentType.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.JobPostRequestStatus))
                    query = query.Where(j => j.ProcessIdNavigation.Status == queryObject.JobPostRequestStatus);
            }
            query = query.Where(r => r.ProcessIdNavigation.RequesterId == userId);

            query = query.OrderByDescending(j => j.ProcessIdNavigation.RequestDate);

            int totalCount = await query.CountAsync();

            query = query.Skip(skipNumber).Take(pageSize);

            var jobPostRequests = await query.ToListAsync();

            return (jobPostRequests, totalCount);
        }

        public async Task<(IEnumerable<JobPostRequest>,int totalCount)> GetAllJobPostRequestHistoryHRManager(int? companyId, JobPostSearch queryObject, Pagination pagination)
        {
            var query = _context.JobPostRequests.AsQueryable();
            var pageSize = pagination.PageSize ?? 10;  // Default page size to 10 if not provided
            var pageNumber = pagination.PageNumber ?? 1;  // Default page number to 1 if not provided

            // Ensure pageSize and pageNumber are valid
            pageSize = Math.Max(1, pageSize);  // Ensure page size is at least 1
            pageNumber = Math.Max(1, pageNumber);  // Ensure page number is at least 1

            var skipNumber = (pageNumber - 1) * pageSize;

            query = query.Include(r => r.ProcessIdNavigation).ThenInclude(p => p.Requester)
                         .Include(r => r.ProcessIdNavigation).ThenInclude(p => p.WorkflowActions);

            if (!string.IsNullOrEmpty(queryObject.Keywords))
            {
                var keyword = queryObject.Keywords.ToLower();
                int.TryParse(queryObject.Keywords, out int keywordAsInt); // Try to parse keyword as integer

                query = query.Where(j =>
                    (j.JobTitle != null && j.JobTitle.ToLower().Contains(keyword)) ||
                    (j.Description != null && j.Description.ToLower().Contains(keyword)) ||
                    (j.Requirements != null && j.Requirements.ToLower().Contains(keyword)) ||
                    (j.EmploymentType != null && j.EmploymentType.ToLower().Contains(keyword)) ||
                    (j.Location != null && j.Location.ToLower().Contains(keyword)) ||
                    (keywordAsInt > 0 && (j.MinSalary == keywordAsInt || j.MaxSalary == keywordAsInt)) // Match numeric salary
                );
            }

            if (queryObject != null)
            {
                if (!string.IsNullOrEmpty(queryObject.JobTitle))
                    query = query.Where(j => j.JobTitle.ToLower().Contains(queryObject.JobTitle.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.Location))
                    query = query.Where(j => j.Location.ToLower().Contains(queryObject.Location.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.Description))
                    query = query.Where(j => j.Description.ToLower().Contains(queryObject.Description.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.Requirement))
                    query = query.Where(j => j.Requirements.ToLower().Contains(queryObject.Requirement.ToLower()));

                if (queryObject.MinSalary != null && queryObject.MinSalary > 0)
                    query = query.Where(j => j.MinSalary >= queryObject.MinSalary);

                if (queryObject.MaxSalary != null && queryObject.MaxSalary > 0)
                    query = query.Where(j => j.MaxSalary <= queryObject.MaxSalary);

                if (!string.IsNullOrEmpty(queryObject.EmploymentType))
                    query = query.Where(j => j.EmploymentType.ToLower().Contains(queryObject.EmploymentType.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.JobPostRequestStatus))
                    query = query.Where(j => j.ProcessIdNavigation.Status == queryObject.JobPostRequestStatus);
            }

            query = query.Where(j =>
                j.CompanyId == companyId &&
                (j.ProcessIdNavigation.WorkflowSequence.RequiredRole == null 
                 // || j.ProcessIdNavigation.WorkflowSequence.Role.Name == "Recruiter"
                 )
                 );

            query = query.OrderByDescending(j => j.ProcessIdNavigation.RequestDate);

            int totalCount = await query.CountAsync();

            query = query.Skip(skipNumber).Take(pageSize);

            var jobPostRequests = await query.ToListAsync();

            return (jobPostRequests, totalCount);
        }
    }
}
