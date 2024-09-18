using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Persistance.Repositories
{
    public class WorkflowSequenceRepository : IWorkflowSequenceRepository
    {
        private readonly AppDbContext _context;

        public WorkflowSequenceRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task CreateAsync(WorkflowSequence entity)
        {
            await _context.WorkflowSequences.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(WorkflowSequence entity)
        {
            _context.WorkflowSequences.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<WorkflowSequence>> GetAllAsync()
        {
            var workflowSequences = await _context.WorkflowSequences.ToListAsync();

            return workflowSequences;
        }

        public async Task<WorkflowSequence?> GetByIdAsync(int id)
        {
            var workflowSequence = await _context.WorkflowSequences.FindAsync(id);

            return workflowSequence;
        }

        public async Task UpdateAsync(WorkflowSequence entity)
        {
            _context.WorkflowSequences.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
