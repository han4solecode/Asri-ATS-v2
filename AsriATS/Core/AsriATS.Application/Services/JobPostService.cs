using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.Services
{
    public class JobPostService : IJobPostService
    {
        private readonly IJobPostRepository _jobPostRepository;

        public JobPostService(IJobPostRepository jobPostRepository)
        {
            _jobPostRepository = jobPostRepository;
        }

        public async Task<IEnumerable<JobPost>> GetAllJobPostAsync(Pagination pagination)
        {
            var jobs = await _jobPostRepository.GetAllAsync();
            var skipNumber = (pagination.PageNumber - 1) * pagination.PageSize;

            // Execute the query asynchronously using ToListAsync()
            return jobs.Skip(skipNumber).Take(pagination.PageSize);
        }

        public async Task<IEnumerable<JobPost>> SeachJobPostAsync(QueryObject queryObject, Pagination pagination)
        {
            // Get the job posts query as an IQueryable (query hasn't been executed yet)
            var jobs = _jobPostRepository.SeachJobPostAsync();

            // Apply filtering based on query operators
            if (queryObject.QueryOperators.Equals("OR", StringComparison.OrdinalIgnoreCase))
            {
                jobs = jobs.Where(j =>
                (!string.IsNullOrEmpty(queryObject.JobTitle) && j.JobTitle.ToLower().Contains(queryObject.JobTitle.ToLower())) ||
                (!string.IsNullOrEmpty(queryObject.Location) && j.Location.ToLower().Contains(queryObject.Location.ToLower())) ||
                (queryObject.MinSalary >= 0 && j.MinSalary >= queryObject.MinSalary) ||
                (queryObject.MaxSalary >= 0 && j.MaxSalary <= queryObject.MaxSalary) ||
                (!string.IsNullOrEmpty(queryObject.EmploymentType) && j.EmploymentType.ToLower().Contains(queryObject.EmploymentType.ToLower()))
                );
            }
            else
            {
                if (!string.IsNullOrEmpty(queryObject.JobTitle))
                    jobs = jobs.Where(j => j.JobTitle.ToLower().Contains(queryObject.JobTitle.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.Location))
                    jobs = jobs.Where(j => j.Location.ToLower().Contains(queryObject.Location.ToLower()));

                if (queryObject.MinSalary > 0)
                    jobs = jobs.Where(j => j.MinSalary >= queryObject.MinSalary);

                if (queryObject.MaxSalary > 0)
                    jobs = jobs.Where(j => j.MaxSalary <= queryObject.MaxSalary);

                if (!string.IsNullOrEmpty(queryObject.EmploymentType))
                    jobs = jobs.Where(j => j.EmploymentType.ToLower().Contains(queryObject.EmploymentType.ToLower()));
            }

            // Apply ordering and pagination
            jobs = jobs.OrderBy(j => j.JobTitle);

            var skipNumber = (pagination.PageNumber - 1) * pagination.PageSize;

            // Execute the query asynchronously using ToListAsync()
            return await jobs.Skip(skipNumber).Take(pagination.PageSize).ToListAsync();
        }
    }
}
