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
    public class DocumentSupportRepository : IDocumentSupportRepository
    {
        private readonly AppDbContext _context;

        public DocumentSupportRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task CreateAsync(SupportingDocument entity)
        {
            await _context.SupportingDocuments.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(SupportingDocument entity)
        {
            _context.SupportingDocuments.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<SupportingDocument>> GetAllAsync()
        {
            var a = await _context.SupportingDocuments.ToListAsync();
            return a;
        }

        public async Task<SupportingDocument?> GetByIdAsync(int id)
        {
            return await _context.SupportingDocuments.FindAsync(id);
        }

        public async Task UpdateAsync(SupportingDocument entity)
        {
            _context.SupportingDocuments.Update(entity);
            await _context.SaveChangesAsync();
        }
    }
}
