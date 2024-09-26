using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AsriATS.Persistance.Repositories
{
    public class ProcessRepository : IProcessRepository
    {
        private readonly AppDbContext _context;

        public ProcessRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task CreateAsync(Process entity)
        {
            await _context.Processes.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Process entity)
        {
            _context.Processes.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Process>> GetAllAsync()
        {
            var processes = await _context.Processes.ToListAsync();

            return processes;
        }

        public async Task<Process?> GetByIdAsync(int id)
        {
            // var process = await _context.Processes.FindAsync(id);
            var process = await _context.Processes.Include(p => p.WorkflowSequence).ThenInclude(ws => ws.Role).Include(p => p.Requester).Include(p => p.WorkflowActions).SingleOrDefaultAsync(p => p.ProcessId == id);

            return process;
        }

        public async Task UpdateAsync(Process entity)
        {
            _context.Processes.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<Process?> GetFirstOrDefaultAsync(Expression<Func<Process, bool>> expression)
        {
            return await _context.Processes.FirstOrDefaultAsync(expression);
        }
    }
}
