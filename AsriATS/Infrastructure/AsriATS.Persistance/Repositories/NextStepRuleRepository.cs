using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace AsriATS.Persistance.Repositories
{
    public class NextStepRuleRepository : INextStepRuleRepository
    {
        private readonly AppDbContext _context;

        public NextStepRuleRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task CreateAsync(NextStepRule entity)
        {
            await _context.NextStepsRules.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(NextStepRule entity)
        {
            _context.NextStepsRules.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<NextStepRule>> GetAllAsync()
        {
            var nextStepRules = await _context.NextStepsRules.ToListAsync();

            return nextStepRules;
        }

        public async Task<NextStepRule?> GetByIdAsync(int id)
        {
            var nextStepRule = await _context.NextStepsRules.FindAsync(id);

            return nextStepRule;
        }

        public async Task UpdateAsync(NextStepRule entity)
        {
            _context.NextStepsRules.Update(entity);
            await _context.SaveChangesAsync();
        }
        public async Task<NextStepRule?> GetFirstOrDefaultAsync(Expression<Func<NextStepRule, bool>> expression)
        {
            return await _context.NextStepsRules.FirstOrDefaultAsync(expression);
        }
    }
}
