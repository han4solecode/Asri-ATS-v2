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
    }
}
