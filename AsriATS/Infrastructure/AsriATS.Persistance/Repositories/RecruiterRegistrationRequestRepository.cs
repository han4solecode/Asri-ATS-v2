using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace AsriATS.Persistance.Repositories
{
    public class RecruiterRegistrationRequestRepository : IRecruiterRegistrationRequestRepository
    {
        private readonly AppDbContext _context;

        public RecruiterRegistrationRequestRepository(AppDbContext appDbContext)
        {
            _context = appDbContext;
        }

        public async Task CreateAsync(RecruiterRegistrationRequest entity)
        {
            await _context.RecruiterRegistrationRequests.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(RecruiterRegistrationRequest entity)
        {
            _context.RecruiterRegistrationRequests.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<RecruiterRegistrationRequest>> GetAllAsync()
        {
            var recruiterRegistrationRequests = await _context.RecruiterRegistrationRequests.ToListAsync();

            return recruiterRegistrationRequests;
        }

        public async Task<RecruiterRegistrationRequest?> GetByIdAsync(int id)
        {
            var recruiterRegistrationRequest = await _context.RecruiterRegistrationRequests.FindAsync(id);

            return recruiterRegistrationRequest;
        }

        public async Task UpdateAsync(RecruiterRegistrationRequest entity)
        {
            _context.RecruiterRegistrationRequests.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
