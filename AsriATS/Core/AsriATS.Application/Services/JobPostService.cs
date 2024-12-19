using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.JobPost;
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

            // If pagination is null or neither pageSize nor pageNumber is provided, return all jobs
            if (pagination == null || (pagination.PageSize == null && pagination.PageNumber == null))
            {
                return jobs; // Return all job posts without pagination
            }

            // Apply default values for pagination if only one of pageSize or pageNumber is provided
            var pageSize = pagination.PageSize ?? 10;  // Default page size to 10 if not provided
            var pageNumber = pagination.PageNumber ?? 1;  // Default page number to 1 if not provided

            // Ensure pageSize and pageNumber are valid
            pageSize = Math.Max(1, pageSize);  // Ensure page size is at least 1
            pageNumber = Math.Max(1, pageNumber);  // Ensure page number is at least 1

            var skipNumber = (pageNumber - 1) * pageSize;

            // Return the paginated result
            return jobs.Skip(skipNumber).Take(pageSize);
        }

        public async Task<IEnumerable<JobPost>> SearchJobPostAsync(QueryObject queryObject, Pagination pagination)
        {
            // Get the job posts and companies
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

                // Filter companies based on the queryObject.CompanyName and get the matching company IDs
                var matchingCompanyIds = company
                    .Where(c => string.IsNullOrEmpty(queryObject.CompanyName) || c.Name.ToLower().Contains(queryObject.CompanyName.ToLower()))
                    .Select(c => c.CompanyId)
                    .ToList();

                // Further filter jobs to match the company IDs
                jobs = jobs.Where(j => matchingCompanyIds.Contains(j.CompanyId)).ToList();
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

                    // Filter company by name and get matching CompanyIds
                    if (!string.IsNullOrEmpty(queryObject.CompanyName))
                    {
                        var matchingCompanyIds = company
                            .Where(c => c.Name.ToLower().Contains(queryObject.CompanyName.ToLower()))
                            .Select(c => c.CompanyId)
                            .ToList();

                        // Filter jobs based on CompanyId
                        jobs = jobs.Where(j => matchingCompanyIds.Contains(j.CompanyId)).ToList();
                    }
                }
            }
            // Sort jobs by CreatedDate (newest first)
            jobs = jobs.OrderByDescending(j => j.CreatedDate).ToList();

            // If pagination is null or neither pageSize nor pageNumber is provided, return all jobs
            if (pagination == null || (pagination.PageSize == null && pagination.PageNumber == null))
            {
                return jobs; // Return all job posts without pagination
            }

            // Apply default values for pagination if only one of pageSize or pageNumber is provided
            var pageSize = pagination.PageSize ?? 10;  // Default page size to 10 if not provided
            var pageNumber = pagination.PageNumber ?? 1;  // Default page number to 1 if not provided

            // Ensure pageSize and pageNumber are valid
            pageSize = Math.Max(1, pageSize);  // Ensure page size is at least 1
            pageNumber = Math.Max(1, pageNumber);  // Ensure page number is at least 1

            var skipNumber = (pageNumber - 1) * pageSize;

            // Return paginated results
            return jobs.Skip(skipNumber).Take(pageSize).ToList();
        }



        // New search Job Post Method
        public async Task<object> SearchJobPostsAsync(JobPostSearch searchParams)
        {
            // Fetch data from repositories
            var jobs = _jobPostRepository.SearchJobPostAsync(); // Fetch job posts
            var companies = _companyRepository.SearchCompanyAsync(); // Fetch companies

            var query = jobs.AsQueryable();

            // Filtering
            if (!string.IsNullOrEmpty(searchParams.JobTitle))
                query = query.Where(j => j.JobTitle.ToLower().Contains(searchParams.JobTitle.ToLower()));

            if (!string.IsNullOrEmpty(searchParams.Location))
                query = query.Where(j => j.Location.ToLower().Contains(searchParams.Location.ToLower()));

            if (!string.IsNullOrEmpty(searchParams.Description))
                query = query.Where(j => j.Description.ToLower().Contains(searchParams.Description.ToLower()));

            if (!string.IsNullOrEmpty(searchParams.Requirement))
                query = query.Where(j => j.Requirements.ToLower().Contains(searchParams.Requirement.ToLower()));

            if (searchParams.MinSalary > 0)
                query = query.Where(j => j.MinSalary >= searchParams.MinSalary);

            if (searchParams.MaxSalary > 0)
                query = query.Where(j => j.MaxSalary <= searchParams.MaxSalary);

            if (!string.IsNullOrEmpty(searchParams.EmploymentType))
                query = query.Where(j => j.EmploymentType.ToLower().Contains(searchParams.EmploymentType.ToLower()));

            if (!string.IsNullOrEmpty(searchParams.CompanyName))
            {
                var matchingCompanyIds = companies
                    .Where(c => c.Name.ToLower().Contains(searchParams.CompanyName.ToLower()))
                    .Select(c => c.CompanyId)
                    .ToList();

                query = query.Where(j => matchingCompanyIds.Contains(j.CompanyId));
            }

            if (!string.IsNullOrEmpty(searchParams.Keywords))
            {
                var keyword = searchParams.Keywords.ToLower();
                int.TryParse(searchParams.Keywords, out int keywordAsInt); // Try to parse keyword as integer

                query = query.Where(j =>
                    (j.JobTitle != null && j.JobTitle.ToLower().Contains(keyword)) ||
                    (j.Description != null && j.Description.ToLower().Contains(keyword)) ||
                    (j.Requirements != null && j.Requirements.ToLower().Contains(keyword)) ||
                    (j.EmploymentType != null && j.EmploymentType.ToLower().Contains(keyword)) ||
                    (j.Location != null && j.Location.ToLower().Contains(keyword)) ||
                    (j.CompanyId != null && companies.Any(c => c.CompanyId == j.CompanyId && c.Name.ToLower().Contains(keyword))) ||
                    (keywordAsInt > 0 && (j.MinSalary == keywordAsInt || j.MaxSalary == keywordAsInt)) // Match numeric salary
                );
            }

            // Sorting
            if (!string.IsNullOrEmpty(searchParams.SortBy))
            {
                query = searchParams.SortBy.ToLower() switch
                {
                    "jobtitle" => searchParams.SortOrder == "desc" ? query.OrderByDescending(j => j.JobTitle) : query.OrderBy(j => j.JobTitle),
                    "location" => searchParams.SortOrder == "desc" ? query.OrderByDescending(j => j.Location) : query.OrderBy(j => j.Location),
                    "employmenttype" => searchParams.SortOrder == "desc" ? query.OrderByDescending(j => j.EmploymentType) : query.OrderBy(j => j.EmploymentType),
                    "minsalary" => searchParams.SortOrder == "desc" ? query.OrderByDescending(j => j.MinSalary) : query.OrderBy(j => j.MinSalary),
                    "maxsalary" => searchParams.SortOrder == "desc" ? query.OrderByDescending(j => j.MaxSalary) : query.OrderBy(j => j.MaxSalary),
                    _ => query.OrderByDescending(j => j.CreatedDate) // Default sort by newest
                };
            }
            else
            {
                query = query.OrderByDescending(j => j.CreatedDate);
            }

            // Pagination
            var totalRecords = query.Count();
            var pageNumber = searchParams.PageNumber ?? 1;
            var pageSize = searchParams.PageSize ?? 20;
            var skip = (pageNumber - 1) * pageSize;

            var paginatedJobs = await Task.FromResult(query.Skip(skip).Take(pageSize).ToList());

            // Map company names to results
            var jobResults = paginatedJobs.Select(j => new
            {
                j.JobPostId,
                j.JobTitle,
                j.Location,
                j.Description,
                j.Requirements,
                j.MinSalary,
                j.MaxSalary,
                j.EmploymentType,
                CompanyName = companies.FirstOrDefault(c => c.CompanyId == j.CompanyId)?.Name ?? "N/A",
                j.CreatedDate
            });
            // Return result
            return new
            {
                TotalRecords = totalRecords,
                Data = jobResults
            };
        }

        public async Task<JobPostDetail> JobPostDetails(int jobPostId)
        {
            // Fetch the JobPost entity by ID
            var job = await _jobPostRepository.GetByIdAsync(jobPostId);

            // Ensure the job exists
            if (job == null)
                throw new Exception("Job post not found");

            // Fetch company details (if needed)
            var company = await _companyRepository.GetByIdAsync(job.CompanyId);

            // Map the JobPost entity to the JobPostDetail DTO
            var jobDetail = new JobPostDetail
            {
                JobPostId = job.JobPostId,
                JobTitle = job.JobTitle,
                Location = job.Location,
                Description = job.Description,
                Requirement = job.Requirements,
                EmploymentType = job.EmploymentType,
                MinSalary = job.MinSalary,
                MaxSalary = job.MaxSalary,
                CompanyName = company?.Name ?? "N/A" // Get company name or default to "N/A"
            };

            return jobDetail;
        }
    }
}
