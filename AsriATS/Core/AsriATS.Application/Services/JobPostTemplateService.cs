using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.JobPost;
using AsriATS.Application.DTOs.JobPostTemplateRequest;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using System.Linq.Expressions;
using System.Security.Claims;

namespace AsriATS.Application.Services
{
    public class JobPostTemplateService :IJobPostTemplateService
    {
        private readonly IJobPostTemplateRepository _jobPostTemplateRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public JobPostTemplateService(IJobPostTemplateRepository jobPostTemplateRepository, ICompanyRepository companyRepository, UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor)
        {
            _jobPostTemplateRepository = jobPostTemplateRepository;
            _companyRepository = companyRepository;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<object> GetAllJobPostTemplate(JobPostSearch jobPostSearch, Pagination pagination)
        {
            var userName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);
            var user = await _userManager.FindByNameAsync(userName!);

            var (jobTemplatePosts, totalCount) = await _jobPostTemplateRepository.GetAllAsync(j => j.CompanyId == user!.CompanyId,jobPostSearch, pagination);
            var jobpostTemplates = jobTemplatePosts.Select(j => new
            {
                JobPostTemplateId = j.JobPostTemplateId,
                JobTitle = j.JobTitle,
                CompanyName = j.CompanyIdNavigation.Name,
                Description = j.Description,
                Requirements = j.Requirements,
                Location = j.Location,
                MinSalary = j.MinSalary,
                MaxSalary = j.MaxSalary,
                EmploymentType = j.EmploymentType
            }).ToList();

            var totalPages = (int)Math.Ceiling((decimal)(totalCount) / (pagination.PageSize ?? 20));

            var result = new
            {
                Data = jobpostTemplates,
                TotalPages = totalPages,
            };

            return result;
        }

        public async Task<JobPostTemplateRequestResponseDto> GetJobPostTemplate(int id)
        {
            var userName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

            // Get the user information from UserManager
            var user = await _userManager.FindByNameAsync(userName);
            if (user == null)
            {
                return new JobPostTemplateRequestResponseDto
                {
                    Status = "Error",
                    Message = "User not found!"
                };
            }

            var jobPostTemplateRequest = await _jobPostTemplateRepository.GetByIdAsync(id);

            if (jobPostTemplateRequest == null)
            {
                return new JobPostTemplateRequestResponseDto
                {
                    Status = "Error",
                    Message = "Job post template is not found"
                };
            }

            if (user.CompanyId != jobPostTemplateRequest.CompanyId)
            {
                return new JobPostTemplateRequestResponseDto
                {
                    Status = "Error",
                    Message = "You are not authorized to view job post template for this company."
                };
            }

            var jobPostTemplateDto = new JobPostTemplateRequestResponseDto
            {
                JobTemplateId=jobPostTemplateRequest.JobPostTemplateId,
                JobTitle = jobPostTemplateRequest.JobTitle,
                CompanyName = jobPostTemplateRequest.CompanyIdNavigation.Name,
                Description = jobPostTemplateRequest.Description,
                Requirements = jobPostTemplateRequest.Requirements,
                Location = jobPostTemplateRequest.Location,
                MinSalary = jobPostTemplateRequest.MinSalary,
                MaxSalary = jobPostTemplateRequest.MaxSalary,
                EmploymentType = jobPostTemplateRequest.EmploymentType,
                Status = "Success",
                Message = "Job post template get successfuly"
            };
            return jobPostTemplateDto;
        }
    }
}
