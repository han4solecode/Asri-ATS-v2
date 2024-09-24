using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsriATS.Persistance.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly AppDbContext _context;

        public CompanyRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task CreateAsync(Company entity)
        {
            await _context.Companies.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Company entity)
        {
            _context.Companies.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Company>> GetAllAsync()
        {
            var companies = await _context.Companies.ToListAsync();

            return companies;
        }

        public async Task<Company?> GetByIdAsync(int id)
        {
            var company = await _context.Companies.FindAsync(id);

            return company;
        }

        public async Task UpdateAsync(Company entity)
        {
            _context.Companies.Update(entity);
            await _context.SaveChangesAsync();
        }

        public IQueryable<Company> SearchCompanyAsync()
        {
            return _context.Companies.AsQueryable();
        }

    }
}