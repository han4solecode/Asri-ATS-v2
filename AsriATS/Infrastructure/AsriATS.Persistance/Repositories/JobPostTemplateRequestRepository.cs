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

        public async Task<IEnumerable<JobPostTemplateRequest>> GetAllToBeReviewed()
        {
            var jobPostTemplateRequests = await _context.JobPostTemplateRequests.Where(x => x.IsApproved == null).ToListAsync();

            return jobPostTemplateRequests;
        }

        public async Task<JobPostTemplateRequest?> GetByIdAsync(int id)
        {
            var jobPostTemplateRequest = await _context.JobPostTemplateRequests.FindAsync(id);

            return jobPostTemplateRequest;
        }

        public async Task UpdateAsync(JobPostTemplateRequest entity)
        {
            _context.JobPostTemplateRequests.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
