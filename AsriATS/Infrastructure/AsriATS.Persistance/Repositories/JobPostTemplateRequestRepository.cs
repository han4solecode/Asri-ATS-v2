using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.JobPost;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace AsriATS.Persistance.Repositories
{
    public class JobPostTemplateRequestRepository : IJobTemplateRequestRepository
    {
        private readonly AppDbContext _context;

        public JobPostTemplateRequestRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task CreateAsync(JobPostTemplateRequest entity)
        {
            await _context.JobPostTemplateRequests.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(JobPostTemplateRequest entity)
        {
            _context.JobPostTemplateRequests.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<JobPostTemplateRequest>> GetAllAsync()
        {
            var jobPostTemplateRequests = await _context.JobPostTemplateRequests.ToListAsync();

            return jobPostTemplateRequests;
        }

        public async Task<(IEnumerable<JobPostTemplateRequest>, int totalCount)> GetAllJobPostTemplateRequest(Expression<Func<JobPostTemplateRequest, bool>>? expression, JobPostSearch? queryObject, Pagination? pagination)
        {
            var query = _context.JobPostTemplateRequests.AsQueryable();
            var pageSize = pagination.PageSize ?? 10;  // Default page size to 10 if not provided
            var pageNumber = pagination.PageNumber ?? 1;  // Default page number to 1 if not provided

            // Ensure pageSize and pageNumber are valid
            pageSize = Math.Max(1, pageSize);  // Ensure page size is at least 1
            pageNumber = Math.Max(1, pageNumber);  // Ensure page number is at least 1

            var skipNumber = (pageNumber - 1) * pageSize;

            query = query.Include(j => j.RequesterIdNavigation);

            if(expression != null)
            {
                query = query.Where(expression);
            }


            if (queryObject != null)
            {
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
            }

            query = query.OrderByDescending(j => j.JobTemplateRequestId);

            int totalCount = await query.CountAsync();

            query = query.Skip(skipNumber).Take(pageSize);

            var jobPostTemplateRequests = await query.ToListAsync();

            return (jobPostTemplateRequests, totalCount);
        }

        public async Task<JobPostTemplateRequest?> GetByIdAsync(int id)
        {
            var jobPostTemplateRequest = await _context.JobPostTemplateRequests.Include(j => j.RequesterIdNavigation).Include(j => j.CompanyIdNavigation).FirstOrDefaultAsync(x => x.JobTemplateRequestId == id);

            return jobPostTemplateRequest;
        }

        public async Task UpdateAsync(JobPostTemplateRequest entity)
        {
            _context.JobPostTemplateRequests.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
