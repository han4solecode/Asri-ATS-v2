using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

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
    }
}
