using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
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

        public async Task<IEnumerable<JobPostTemplate>> GetAllAsync(Expression<Func<JobPostTemplate, bool>> expression)
        {
            var jobPostTemplates = await _context.JobPostTemplates.Where(expression).Include(j => j.CompanyIdNavigation).ToListAsync();

            return jobPostTemplates;
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