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

        public async Task<IEnumerable<InterviewScheduling>> GetAllInterviewSchedulesAsync(Expression<Func<InterviewScheduling, bool>> expression)
        {
            return await _context.InterviewScheduling
                .Include(i => i.ApplicationIdNavigation)
                .ThenInclude(aj => aj.JobPostNavigation)
                .Include( i => i.ApplicationIdNavigation)
                .ThenInclude(aj => aj.UserIdNavigation)
                .Where(expression) // Filter
                .ToListAsync();
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
