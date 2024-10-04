using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Persistance.Repositories
{
    public class ApplicationJobRepository:IApplicationJobRepository
    {
        private readonly AppDbContext _context;

        public ApplicationJobRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(ApplicationJob entity)
        {
            await _context.ApplicationJobs.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(ApplicationJob entity)
        {
            _context.ApplicationJobs.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ApplicationJob>> GetAllAsync()
        {
            var a = await _context.ApplicationJobs.ToListAsync();
            return a;
        }

        public async Task<ApplicationJob?> GetByIdAsync(int id)
        {
            return await _context.ApplicationJobs.Include(aj => aj.JobPostNavigation).FirstOrDefaultAsync(aj => aj.ApplicationJobId == id);
        }

        public async Task UpdateAsync(ApplicationJob entity)
        {
            _context.ApplicationJobs.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<ApplicationJob>> GetAllToStatusAsync(int companyId, string userRole)
        {
            // Retrieve all applications for the user's company where the workflow role matches
            var applications = await _context.ApplicationJobs
                .Include(app => app.ProcessIdNavigation)
                .ThenInclude(p => p.WorkflowActions)
                .Include(app => app.JobPostNavigation)
                .Where(app => app.JobPostNavigation.CompanyId == companyId &&
                              app.ProcessIdNavigation.WorkflowSequence.RequiredRole == userRole)
                .ToListAsync();

            return applications;
        }

        public async Task<IEnumerable<ApplicationJob>> GetAllByApplicantAsync(Expression<Func<ApplicationJob, bool>> expression)
        {
            return await _context.ApplicationJobs
                .Include(r => r.ProcessIdNavigation)
                .ThenInclude(p => p.WorkflowSequence)
                .ThenInclude(wfs => wfs.Role)
                .Include(r => r.ProcessIdNavigation)
                .ThenInclude(p => p.Requester)
                .Include(r => r.ProcessIdNavigation)
                .ThenInclude(p => p.WorkflowActions)
                .Include(r => r.JobPostNavigation)
                .ThenInclude(p => p.CompanyIdNavigation)
                .Include(r => r.SupportingDocumentsIdNavigation)
                .Where(expression) // Filter
                .ToListAsync();
        }

        public async Task<ApplicationJob?> GetFirstOrDefaultAsync(Expression<Func<ApplicationJob, bool>> predicate)
        {
            return await _context.ApplicationJobs.FirstOrDefaultAsync(predicate);
        }

        public async Task<ApplicationJob> GetFirstOrDefaultAsyncUpdate(Expression<Func<ApplicationJob, bool>> predicate, Func<IQueryable<ApplicationJob>, IIncludableQueryable<ApplicationJob, object>> include = null)
        {
            IQueryable<ApplicationJob> query = _context.ApplicationJobs;

            if (include != null)
            {
                query = include(query); // Apply eager loading
            }

            return await query.FirstOrDefaultAsync(predicate);
        }
    }
}
