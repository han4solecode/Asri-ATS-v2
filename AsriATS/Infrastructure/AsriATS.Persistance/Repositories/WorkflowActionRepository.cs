using AsriATS.Application.DTOs.WorkflowAction;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AsriATS.Persistance.Repositories
{
    public class WorkflowActionRepository : IWorkflowActionRepository
    {
        private readonly AppDbContext _context;

        public WorkflowActionRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task CreateAsync(WorkflowAction entity)
        {
            await _context.WorkflowActions.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(WorkflowAction entity)
        {
            _context.WorkflowActions.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<WorkflowAction>> GetAllAsync()
        {
            var workflowActions = await _context.WorkflowActions.ToListAsync();

            return workflowActions;
        }

        public async Task<IEnumerable<WorkflowAction>> GetAllAsync(Expression<Func<WorkflowAction, bool>> expression)
        {
            var workflowActions = await _context.WorkflowActions.Include(w => w.Actor).Where(expression).ToListAsync();

            return workflowActions;
        }

        public async Task<WorkflowAction?> GetByIdAsync(int id)
        {
            var workflowAction = await _context.WorkflowActions.FindAsync(id);

            return workflowAction;
        }

        public async Task<WorkflowAction?> GetFirstOrDefaultAsync(Expression<Func<WorkflowAction, bool>> expression)
        {
            return await _context.WorkflowActions.FirstOrDefaultAsync(expression);
        }

        public async Task UpdateAsync(WorkflowAction entity)
        {
            _context.WorkflowActions.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RecruitmentFunnelDto>> GetRecruitmentFlunnel(int? companyId)
        {
            var recruitmentFunnel = await _context.WorkflowActions
                .Include(wa => wa.Process)                           
                    .ThenInclude(p => p.ApplicationJobNavigation)
                     .ThenInclude(aj => aj.JobPostNavigation)            
                     .ThenInclude(jp => jp.CompanyIdNavigation)
                .Include(wa => wa.WorkflowSequence)                       
                .Where(wa => wa.Process.ApplicationJobNavigation.Any(aj => aj.JobPostNavigation.CompanyId == companyId))
                .GroupBy(wa => new { CompanyName = wa.Process.ApplicationJobNavigation.FirstOrDefault().JobPostNavigation.CompanyIdNavigation.Name, wa.WorkflowSequence.StepName })
                .Select(g => new RecruitmentFunnelDto
                {
                    CompanyName = g.Key.CompanyName,
                    StageName = g.Key.StepName,
                    Count = g.Count()
                })
                .ToListAsync();
            return recruitmentFunnel;
        }

        public async Task<List<WorkflowAction>> GetByProcessIdAsync(int processId)
        {
            var workflowActions = await _context.WorkflowActions
                                                .Where(w => w.ProcessId == processId)
                                                .Include(w => w.Actor)
                                                .OrderByDescending(w => w.ActionDate)
                                                .ToListAsync();

            return workflowActions;
        }
    }
}
