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

            // Apply default values for pagination if not provided
            var pageSize = pagination?.PageSize ?? 10;  // Default page size to 10 if not provided
            var pageNumber = pagination?.PageNumber ?? 1;  // Default page number to 1 if not provided

            // Ensure pageSize and pageNumber are valid
            pageSize = Math.Max(1, pageSize);  // Ensure page size is at least 1
            pageNumber = Math.Max(1, pageNumber);  // Ensure page number is at least 1

            var skipNumber = (pageNumber - 1) * pageSize;

            // Execute the query asynchronously using ToListAsync()
            return jobs.Skip(skipNumber).Take(pageSize);
        }

        public async Task<IEnumerable<JobPost>> SearchJobPostAsync(QueryObject queryObject, Pagination pagination)
        {
            // Get the job posts query as an IEnumerable
            var jobs = _jobPostRepository.SearchJobPostAsync().ToList();
            var company = _companyRepository.SearchCompanyAsync().ToList();

            // Apply filtering based on query operators
            if (queryObject != null && queryObject.QueryOperators?.Equals("OR", StringComparison.OrdinalIgnoreCase) == true)
            {
                jobs = jobs.Where(j =>
                (string.IsNullOrEmpty(queryObject.JobTitle) || j.JobTitle.ToLower().Contains(queryObject.JobTitle.ToLower())) ||
                (string.IsNullOrEmpty(queryObject.Location) || j.Location.ToLower().Contains(queryObject.Location.ToLower())) ||
                (string.IsNullOrEmpty(queryObject.Description) || j.Description.ToLower().Contains(queryObject.Description.ToLower())) ||
                (string.IsNullOrEmpty(queryObject.Requirement) || j.Requirements.ToLower().Contains(queryObject.Requirement.ToLower())) ||
                (queryObject.MinSalary == null || j.MinSalary >= queryObject.MinSalary) ||
                (queryObject.MaxSalary == null || j.MaxSalary <= queryObject.MaxSalary) ||
                (string.IsNullOrEmpty(queryObject.EmploymentType) || j.EmploymentType.ToLower().Contains(queryObject.EmploymentType.ToLower()))
                ).ToList();

                company = company.Where(c =>
                (string.IsNullOrEmpty(queryObject.CompanyName) || c.Name.ToLower().Contains(queryObject.CompanyName.ToLower()))
                ).ToList();
            }
            else
            {
                if (queryObject != null)
                {
                    if (!string.IsNullOrEmpty(queryObject.JobTitle))
                        jobs = jobs.Where(j => j.JobTitle.ToLower().Contains(queryObject.JobTitle.ToLower())).ToList();

                    if (!string.IsNullOrEmpty(queryObject.Location))
                        jobs = jobs.Where(j => j.Location.ToLower().Contains(queryObject.Location.ToLower())).ToList();

                    if (!string.IsNullOrEmpty(queryObject.Description))
                        jobs = jobs.Where(j => j.Description.ToLower().Contains(queryObject.Description.ToLower())).ToList();

                    if (!string.IsNullOrEmpty(queryObject.Requirement))
                        jobs = jobs.Where(j => j.Requirements.ToLower().Contains(queryObject.Requirement.ToLower())).ToList();

                    if (queryObject.MinSalary != null && queryObject.MinSalary > 0)
                        jobs = jobs.Where(j => j.MinSalary >= queryObject.MinSalary).ToList();

                    if (queryObject.MaxSalary != null && queryObject.MaxSalary > 0)
                        jobs = jobs.Where(j => j.MaxSalary <= queryObject.MaxSalary).ToList();

                    if (!string.IsNullOrEmpty(queryObject.EmploymentType))
                        jobs = jobs.Where(j => j.EmploymentType.ToLower().Contains(queryObject.EmploymentType.ToLower())).ToList();

                    if (!string.IsNullOrEmpty(queryObject.CompanyName))
                        company = company.Where(c => c.Name.ToLower().Contains(queryObject.CompanyName.ToLower())).ToList();
                }
            }

            // Apply default values for pagination if not provided
            var pageSize = pagination?.PageSize ?? 10;  // Default page size to 10 if not provided
            var pageNumber = pagination?.PageNumber ?? 1;  // Default page number to 1 if not provided

            // Ensure pageSize and pageNumber are valid
            pageSize = Math.Max(1, pageSize);  // Ensure page size is at least 1
            pageNumber = Math.Max(1, pageNumber);  // Ensure page number is at least 1

            // Apply ordering
            jobs = jobs.OrderBy(j => j.JobTitle).ToList();

            var skipNumber = (pageNumber - 1) * pageSize;

            // Return paginated results
            return jobs.Skip(skipNumber).Take(pageSize).ToList();
        }
    }
}
