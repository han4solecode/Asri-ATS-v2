using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsriATS.Persistance.Repositories
{
    public class JobPostRepository : IJobPostRepository
    {
        private readonly AppDbContext _context;

        public JobPostRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task CreateAsync(JobPost entity)
        {
            await _context.JobPosts.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(JobPost entity)
        {
            _context.JobPosts.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<JobPost>> GetAllAsync()
        {
            var jobPosts = await _context.JobPosts.ToListAsync();

            return jobPosts;
        }

        public async Task<JobPost?> GetByIdAsync(int id)
        {
            var jobPost = await _context.JobPosts.FindAsync(id);

            return jobPost;
        }

        public async Task UpdateAsync(JobPost entity)
        {
            _context.JobPosts.Update(entity);
            await _context.SaveChangesAsync();
        }

        public IQueryable<JobPost> SearchJobPostAsync()
        {
            return _context.JobPosts.AsQueryable();
        }
    }
}
