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
            return await _context.JobPostRequests.FirstOrDefaultAsync(expression);
        }
    }
}
