using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.JobPostRequest;
using AsriATS.Application.DTOs;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using AsriATS.Application.DTOs.Register;
using AsriATS.Application.DTOs.JobPostTemplateRequest;

namespace AsriATS.Application.Services
{
    public class JobPostTemplateRequestService : IJobPostTemplateRequestService
    {
        private readonly IJobTemplateRequestRepository _jobTemplateRequestRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly UserManager<AppUser> _userManager;
        public JobPostTemplateRequestService(IJobTemplateRequestRepository jobTemplateRequestRepository, ICompanyRepository companyRepository, UserManager<AppUser> userManager)
        {
            _jobTemplateRequestRepository = jobTemplateRequestRepository;
            _companyRepository = companyRepository;
            _userManager = userManager;
        }

        public async Task<BaseResponseDto> SubmitJobTemplateRequest(JobPostTemplateRequestDto request)
        {
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);
            if (company == null)
            {
                return new RegisterResponseDto
                {
                    Status = "Error",
                    Message = "Company is not registered!"
                };
            }

            var newJobTemplateRequest = new JobPostTemplateRequest
            {
                JobTitle = request.JobTitle,
                CompanyId = request.CompanyId,
                Description = request.Description,
                Requirements = request.Requirements,
                Location = request.Location,
                MinSalary = request.MinSalary,
                MaxSalary = request.MaxSalary,
                EmploymentType = request.EmploymentType
            };

            await _jobTemplateRequestRepository.CreateAsync(newJobTemplateRequest);

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Job Template Request created successfully"
            };
        }
    }
}
