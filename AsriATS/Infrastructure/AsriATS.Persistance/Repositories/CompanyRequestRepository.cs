using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsriATS.Persistance.Repositories
{
    public class CompanyRequestRepository : ICompanyRequestRepository
    {
        private readonly AppDbContext _context;

        public CompanyRequestRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task CreateAsync(CompanyRequest entity)
        {
            await _context.CompanyRequests.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(CompanyRequest entity)
        {
            _context.CompanyRequests.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<CompanyRequest>> GetAllAsync()
        {
            var companyRequests = await _context.CompanyRequests.ToListAsync();

            return companyRequests;
        }

        public async Task<CompanyRequest?> GetByIdAsync(int id)
        {
            var companyRequest = await _context.CompanyRequests.FindAsync(id);

            return companyRequest;
        }

        public async Task UpdateAsync(CompanyRequest entity)
        {
            _context.CompanyRequests.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}