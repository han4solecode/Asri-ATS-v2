using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsriATS.Persistance.Repositories
{
    public class RoleChangeRequestRepository : IRoleChangeRequestRepository
    {
        private readonly AppDbContext _context;

        public RoleChangeRequestRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task CreateAsync(RoleChangeRequest entity)
        {
            await _context.RoleChangeRequests.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(RoleChangeRequest entity)
        {
            _context.RoleChangeRequests.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RoleChangeRequest>> GetAllAsync()
        {
            var roleChangeRequests = await _context.RoleChangeRequests.ToListAsync();

            return roleChangeRequests;
        }

        public async Task<RoleChangeRequest?> GetByIdAsync(int id)
        {
            var roleChangeRequest = await _context.RoleChangeRequests.FindAsync(id);

            return roleChangeRequest;
        }

        public async Task UpdateAsync(RoleChangeRequest entity)
        {
            _context.RoleChangeRequests.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}