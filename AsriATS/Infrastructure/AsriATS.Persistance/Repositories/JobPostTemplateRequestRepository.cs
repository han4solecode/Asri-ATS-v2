using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
            await _context.JobTemplateRequests.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(JobPostTemplateRequest entity)
        {
            _context.JobTemplateRequests.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<JobPostTemplateRequest>> GetAllAsync()
        {
            var jobTemplateRequests = await _context.JobTemplateRequests.ToListAsync();

            return jobTemplateRequests;
        }

        public async Task<JobPostTemplateRequest?> GetByIdAsync(int id)
        {
            var jobTemplateRequest = await _context.JobTemplateRequests.FindAsync(id);

            return jobTemplateRequest;
        }

        public async Task UpdateAsync(JobPostTemplateRequest entity)
        {
            _context.JobTemplateRequests.Update(entity);
            await _context.SaveChangesAsync();
        }

    }
}
