using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.JobPost;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq;
using System.Linq.Expressions;

namespace AsriATS.Persistance.Repositories
{
    public class JobPostTemplateRepository : IJobPostTemplateRepository
    {
        private readonly AppDbContext _context;

        public JobPostTemplateRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task CreateAsync(JobPostTemplate entity)
        {
            await _context.JobPostTemplates.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(JobPostTemplate entity)
        {
            _context.JobPostTemplates.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<JobPostTemplate>> GetAllAsync()
        {
            var jobPostTemplates = await _context.JobPostTemplates.Include(t => t.CompanyIdNavigation).ToListAsync();

            return jobPostTemplates;
        }

        public async Task<(IEnumerable<JobPostTemplate>, int totalCount)> GetAllAsync(Expression<Func<JobPostTemplate, bool>> expression, JobPostSearch? queryObject, Pagination? pagination)
        {
            var query = _context.JobPostTemplates.AsQueryable();
            var pageSize = pagination.PageSize ?? 10;  // Default page size to 10 if not provided
            var pageNumber = pagination.PageNumber ?? 1;  // Default page number to 1 if not provided

            // Ensure pageSize and pageNumber are valid
            pageSize = Math.Max(1, pageSize);  // Ensure page size is at least 1
            pageNumber = Math.Max(1, pageNumber);  // Ensure page number is at least 1

            var skipNumber = (pageNumber - 1) * pageSize;

            query = query.Include(j => j.CompanyIdNavigation);

            if (expression != null)
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

            query = query.OrderByDescending(j => j.JobPostTemplateId);

            int totalCount = await query.CountAsync();

            query = query.Skip(skipNumber).Take(pageSize);

            var jobPostTemplates = await query.ToListAsync();

            return (jobPostTemplates, totalCount);
        }

        public async Task<JobPostTemplate?> GetByIdAsync(int id)
        {
            var jobPostTemplate = await _context.JobPostTemplates.Include(t => t.CompanyIdNavigation).FirstOrDefaultAsync(x => x.JobPostTemplateId == id);

            return jobPostTemplate;
        }

        public async Task UpdateAsync(JobPostTemplate entity)
        {
            _context.JobPostTemplates.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}