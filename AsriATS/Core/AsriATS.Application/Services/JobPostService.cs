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
        private readonly ICompanyRepository _companyRepository;

        public JobPostService(IJobPostRepository jobPostRepository, ICompanyRepository companyRepository)
        {
            _jobPostRepository = jobPostRepository;
            _companyRepository = companyRepository;
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
            var company = _companyRepository.SearchCompanyAsync();

            // Apply filtering based on query operators
            if (queryObject.QueryOperators.Equals("OR", StringComparison.OrdinalIgnoreCase))
            {
                jobs = jobs.Where(j =>
                (!string.IsNullOrEmpty(queryObject.JobTitle) && j.JobTitle.ToLower().Contains(queryObject.JobTitle.ToLower())) ||
                (!string.IsNullOrEmpty(queryObject.Location) && j.Location.ToLower().Contains(queryObject.Location.ToLower())) ||
                (!string.IsNullOrEmpty(queryObject.Description) && j.Description.ToLower().Contains(queryObject.Description.ToLower())) ||
                (!string.IsNullOrEmpty(queryObject.Requirement) && j.Requirements.ToLower().Contains(queryObject.Requirement.ToLower())) ||
                (queryObject.MinSalary >= 0 && j.MinSalary >= queryObject.MinSalary) ||
                (queryObject.MaxSalary >= 0 && j.MaxSalary <= queryObject.MaxSalary) ||
                (!string.IsNullOrEmpty(queryObject.EmploymentType) && j.EmploymentType.ToLower().Contains(queryObject.EmploymentType.ToLower())) 
                );

                company = company.Where(c =>
                (!string.IsNullOrEmpty(queryObject.CompanyName) && c.Name.ToLower().Contains(queryObject.CompanyName.ToLower()))
                );
            }
            else
            {
                if (!string.IsNullOrEmpty(queryObject.JobTitle))
                    jobs = jobs.Where(j => j.JobTitle.ToLower().Contains(queryObject.JobTitle.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.Location))
                    jobs = jobs.Where(j => j.Location.ToLower().Contains(queryObject.Location.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.Description))
                    jobs = jobs.Where(j => j.Description.ToLower().Contains(queryObject.Description.ToLower()));

                if (!string.IsNullOrEmpty(queryObject.Requirement))
                    jobs = jobs.Where(j => j.Requirements.ToLower().Contains(queryObject.Requirement.ToLower()));

                if (queryObject.MinSalary > 0)
                    jobs = jobs.Where(j => j.MinSalary >= queryObject.MinSalary);

                if (queryObject.MaxSalary > 0)
                    jobs = jobs.Where(j => j.MaxSalary <= queryObject.MaxSalary);

                if (!string.IsNullOrEmpty(queryObject.EmploymentType))
                    jobs = jobs.Where(j => j.EmploymentType.ToLower().Contains(queryObject.EmploymentType.ToLower()));

                if(!string.IsNullOrEmpty(queryObject.CompanyName))
                    company = company.Where(c => c.Name.ToLower().Contains(queryObject.CompanyName.ToLower()));
            }

            // Apply ordering and pagination
            jobs = jobs.OrderBy(j => j.JobTitle);

            var skipNumber = (pagination.PageNumber - 1) * pagination.PageSize;

            // Execute the query asynchronously using ToListAsync()
            return await jobs.Skip(skipNumber).Take(pagination.PageSize).ToListAsync();
        }
    }
}
