using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AsriATS.Persistance.Repositories
{
    public class InterviewSchedulingRepository : IInterviewSchedulingRepository
    {
        private readonly AppDbContext _context;

        public InterviewSchedulingRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task CreateAsync(InterviewScheduling entity)
        {
            await _context.InterviewScheduling.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(InterviewScheduling entity)
        {
            _context.InterviewScheduling.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<InterviewScheduling>> GetAllAsync()
        {
            var interviewSchedules = await _context.InterviewScheduling.ToListAsync();

            return interviewSchedules;
        }

        public async Task<(IEnumerable<InterviewScheduling>, int totalCount)> GetAllInterviewSchedulesAsync(Expression<Func<InterviewScheduling, bool>> expression, Pagination? pagination)
        {
            var query = _context.InterviewScheduling.AsQueryable();
            var pageSize = pagination.PageSize ?? 10;  // Default page size to 10 if not provided
            var pageNumber = pagination.PageNumber ?? 1;  // Default page number to 1 if not provided

            // Ensure pageSize and pageNumber are valid
            pageSize = Math.Max(1, pageSize);  // Ensure page size is at least 1
            pageNumber = Math.Max(1, pageNumber);  // Ensure page number is at least 1

            var skipNumber = (pageNumber - 1) * pageSize;

            query = query.Include(i => i.ApplicationIdNavigation).ThenInclude(aj => aj.JobPostNavigation)
                          .Include(i => i.ApplicationIdNavigation).ThenInclude(aj => aj.UserIdNavigation);

            if (expression != null) {
                query = query.Where(expression);
            }
            

            query = query.OrderByDescending(i => i.InterviewTime);

            int totalCount = await query.CountAsync();

            query = query.Skip(skipNumber).Take(pageSize);

            var interviewSchedules = await query.ToListAsync();

            return (interviewSchedules, totalCount);
        }

        public async Task<InterviewScheduling?> GetByIdAsync(int id)
        {
            var interviewSchedule = await _context.InterviewScheduling.FindAsync(id);

            return interviewSchedule;
        }

        public async Task UpdateAsync(InterviewScheduling entity)
        {
            _context.InterviewScheduling.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<List<InterviewScheduling>> GetByProcessIdAsync(int processId)
        {
            var workflowActions = await _context.InterviewScheduling
                                                .Where(w => w.ProcessId == processId)
                                                .Include(w => w.ProcessIdNavigation) // Include only the navigation property
                                                .OrderByDescending(w => w.ProcessId)
                                                .ToListAsync();

            return workflowActions;
        }
    }
}
