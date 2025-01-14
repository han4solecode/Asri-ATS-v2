using AsriATS.Application.DTOs.Report;
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
               .Include(aj => aj.ProcessIdNavigation)
                   .ThenInclude(p => p.WorkflowSequence)
                   .ThenInclude(wfs => wfs.Role)
               .Include(aj => aj.ProcessIdNavigation)
                   .ThenInclude(p => p.Requester)
               .Include(aj => aj.ProcessIdNavigation)
                   .ThenInclude(p => p.WorkflowActions)
                   .ThenInclude(wa => wa.WorkflowSequence)
                   .ThenInclude(wfs => wfs.Role)
               .Include(aj => aj.JobPostNavigation)
               .Where(aj =>
                   aj.JobPostNavigation.CompanyId == companyId &&
                   (
                       // Applications needing approval by the user's role
                       aj.ProcessIdNavigation.WorkflowSequence.Role.Name == userRole ||

                       // Applications already approved by the user's role
                       aj.ProcessIdNavigation.WorkflowActions.Any(wa =>
                           wa.WorkflowSequence.Role.Name == userRole 
                       )
                   ))
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

        public async Task<int> TotalApplicationJob()
        {
            var apply = await _context.ApplicationJobs.CountAsync();
            return apply;
        }

        // Repository Method for Applications Summary
        public async Task<List<ApplicationJobStatusDto>> GetApplicationSummaryAsync()
        {
            return await (from aj in _context.ApplicationJobs
                          join p in _context.Processes on aj.ProcessId equals p.ProcessId // Join with Process table using ProcessId
                          join isch in _context.InterviewScheduling on aj.ApplicationJobId equals isch.ApplicationId into interviewGroup // Left join with InterviewScheduling using ApplicationJobId
                          from interview in interviewGroup.DefaultIfEmpty() // This ensures a left join
                          select new ApplicationJobStatusDto
                          {
                              ApplicationJobId = aj.ApplicationJobId,
                              JobPostId = aj.JobPostId,
                              UploadedDate = aj.UploadedDate,
                              Status = p.Status, // Status from the Process table
                              InterviewScheduled = interview != null, // Check if an interview is scheduled
                              InterviewDate = interview.InterviewTime // Nullable InterviewDate from InterviewScheduling
                          })
                         .ToListAsync();
        }

        public async Task<IEnumerable<ApplicationJob>> ListAllToStatusAsync(int companyId, string userRole)
        {
            // Retrieve all applications for the user's company where the workflow role matches
            var applications = await _context.ApplicationJobs
                .Include(r => r.ProcessIdNavigation)
                    .ThenInclude(p => p.WorkflowSequence)
                    .ThenInclude(wfs => wfs.Role)
                .Include(r => r.ProcessIdNavigation)
                    .ThenInclude(p => p.Requester)
                .Include(app => app.ProcessIdNavigation)
                    .ThenInclude(p => p.WorkflowActions)
                .Include(app => app.JobPostNavigation)
                .Where(app => app.JobPostNavigation.CompanyId == companyId &&
                              app.ProcessIdNavigation.WorkflowSequence.Role.Name == userRole)
                .ToListAsync();

            return applications;
        }

        public async Task<int> CountApplicationsWithOfferStatusByHRAsync(int companyId)
        {
            var count = await _context.ApplicationJobs
                .Include(aj => aj.ProcessIdNavigation)
                    .ThenInclude(p => p.WorkflowActions) // Include WorkflowActions
                .Include(aj => aj.JobPostNavigation) // Include JobPost for CompanyId
                .Where(aj =>
                    aj.JobPostNavigation.CompanyId == companyId && // Company filter
                    aj.ProcessIdNavigation.WorkflowActions
                        .Any(wa =>
                            wa.Action == "Offer" && // Action type
                            wa.WorkflowSequence.Role.Name == "HR Manager" // Role filter
                        )
                )
                .CountAsync(); // Count the matching applications

            return count;
        }

        public async Task<int> CountApplicationsWithSubmitStatusAsync(int companyId)
        {
            var count = await _context.ApplicationJobs
               .Include(aj => aj.ProcessIdNavigation)
                   .ThenInclude(p => p.WorkflowActions) // Include WorkflowActions
               .Include(aj => aj.JobPostNavigation) // Include JobPost for CompanyId
               .Where(aj =>
                   aj.JobPostNavigation.CompanyId == companyId && // Company filter
                   aj.ProcessIdNavigation.WorkflowActions
                       .Any(wa =>
                           wa.Action == "Submitted" // Action type filter
                       )
               )
               .CountAsync(); // Count the matching applications

            return count;
        }

        public async Task<double> CalculateAverageTimeToHireAsync(int companyId)
        {
            try
            {
                // Fetch relevant applications with a "Hired" status
                var durations = await _context.ApplicationJobs
                    .Where(aj =>
                        aj.JobPostNavigation.CompanyId == companyId && // Filter by company
                        aj.ProcessIdNavigation.Status == "Offer by HR Manager") // Filter by status
                    .Select(aj => new
                    {
                        CreatedDate = aj.JobPostNavigation.CreatedDate,
                        HiredDate = aj.ProcessIdNavigation.RequestDate
                    })
                    .ToListAsync();

                if (durations.Count == 0)
                {
                    return 0; // Return 0 if no "Hired" applications are found
                }

                // Calculate time difference in days for each application
                var timeDifferences = durations
                    .Select(d => (d.HiredDate - d.CreatedDate).TotalDays);

                // Calculate the average time to hire
                return timeDifferences.Average();
            }
            catch (Exception ex)
            {
                // Log the error (example: using a logging framework like Serilog or NLog)
                Console.Error.WriteLine($"Error calculating average time to hire: {ex.Message}");
                throw;
            }
        }

        public async Task<Dictionary<string, int>> GetApplicationStatusCountsAsync(int companyId)
        {
            // Query to group applications by status and count them
            var statusCounts = await _context.ApplicationJobs
                .Include(aj => aj.ProcessIdNavigation)
                .Include(aj => aj.JobPostNavigation) // Include JobPost for CompanyId
                .Where(aj => aj.JobPostNavigation.CompanyId == companyId) // Filter by CompanyId
                .GroupBy(aj => aj.ProcessIdNavigation.Status) // Group by Status
                .Select(group => new
                {
                    Status = group.Key, // Status value
                    Count = group.Count() // Count of applications for this status
                })
                .ToDictionaryAsync(x => x.Status, x => x.Count); // Convert to dictionary

            return statusCounts;
        }

        public async Task<IEnumerable<ApplicationJob>> GetAllToStatusAsync(string userRole)
        {
            return await _context.ApplicationJobs
                .Include(r => r.ProcessIdNavigation)
                    .ThenInclude(p => p.WorkflowSequence)
                    .ThenInclude(wfs => wfs.Role)
                .Include(r => r.ProcessIdNavigation)
                    .ThenInclude(p => p.Requester)
                .Include(r => r.ProcessIdNavigation)
                    .ThenInclude(p => p.WorkflowActions)
                .Include(r => r.JobPostNavigation) // Include JobPostNavigation
                .Where(r => r.ProcessIdNavigation.WorkflowSequence.Role.Name == userRole)
                .ToListAsync();
        }

    }
}
