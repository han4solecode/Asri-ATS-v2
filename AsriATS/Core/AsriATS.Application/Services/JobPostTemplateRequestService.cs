using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Identity;
using AsriATS.Application.DTOs.Register;
using AsriATS.Application.DTOs.JobPostTemplateRequest;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;
using AsriATS.Application.DTOs.Email;

namespace AsriATS.Application.Services
{
    public class JobPostTemplateRequestService : IJobPostTemplateRequestService
    {
        private readonly IJobTemplateRequestRepository _jobTemplateRequestRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IJobPostTemplateRepository _jobPostTemplateRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;

        public JobPostTemplateRequestService(IJobTemplateRequestRepository jobTemplateRequestRepository, ICompanyRepository companyRepository, UserManager<AppUser> userManager, IJobPostTemplateRepository jobPostTemplateRepository, IHttpContextAccessor httpContextAccessor, IEmailService emailService)
        {
            _jobTemplateRequestRepository = jobTemplateRequestRepository;
            _companyRepository = companyRepository;
            _userManager = userManager;
            _jobPostTemplateRepository = jobPostTemplateRepository;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
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

            // Get the current logged-in user
            var userName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

            // Get the user information from UserManager
            var user = await _userManager.FindByNameAsync(userName!);
            if (user == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "User not found!"
                };
            }

            if (user.CompanyId != request.CompanyId)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "You are not authorized to submit job post template for this company."
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
                EmploymentType = request.EmploymentType,
                RequesterId = user.Id
            };

            await _jobTemplateRequestRepository.CreateAsync(newJobTemplateRequest);

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Job Template Request created successfully"
            };
        }

        public async Task<IEnumerable<JobPostTemplateRequest>> GetAllJobPostTemplateRequestToReview()
        {
            var jobPostTemplateRequestToReview = await _jobTemplateRequestRepository.GetAllToBeReviewed();

            return jobPostTemplateRequestToReview;
        }

        public async Task<BaseResponseDto> ReviewJobPostTemplateRequest(JobPostTemplateReviewDto jobPostTemplateReview)
        {
            var request = await _jobTemplateRequestRepository.GetByIdAsync(jobPostTemplateReview.JobTemplateRequestId);

            if (request == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Job post template request does not exsit"
                };
            }

            if (request.IsApproved != null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Request already finished"
                };
            }

            if (jobPostTemplateReview.Action == "Approved")
            {
                // update request
                request.IsApproved = true;
                await _jobTemplateRequestRepository.UpdateAsync(request);

                // create new job post template
                var newJobPostTemplate = new JobPostTemplate
                {
                    JobTitle = request.JobTitle,
                    CompanyId = request.CompanyId,
                    Description = request.Description,
                    Requirements = request.Requirements,
                    Location = request.Location,
                    MinSalary = request.MinSalary,
                    MaxSalary = request.MaxSalary,
                    EmploymentType = request.EmploymentType,
                };

                await _jobPostTemplateRepository.CreateAsync(newJobPostTemplate);

                // send email notification
                var emailTemplate = File.ReadAllText(@"./Templates/EmailTemplates/JobPostTemplateRequestApproved.html");

                emailTemplate = emailTemplate.Replace("{{Name}}", $"{request.RequesterIdNavigation.FirstName} {request.RequesterIdNavigation.LastName}");
                emailTemplate = emailTemplate.Replace("{{CompanyName}}", request.CompanyIdNavigation.Name);
                emailTemplate = emailTemplate.Replace("{{JobTitle}}", newJobPostTemplate.JobTitle);
                emailTemplate = emailTemplate.Replace("{{Description}}", newJobPostTemplate.Description);
                emailTemplate = emailTemplate.Replace("{{Requirements}}", newJobPostTemplate.Requirements);
                emailTemplate = emailTemplate.Replace("{{Location}}", newJobPostTemplate.Location);
                emailTemplate = emailTemplate.Replace("{{SalaryRange}}", $"{newJobPostTemplate.MinSalary} - {newJobPostTemplate.MaxSalary}");
                emailTemplate = emailTemplate.Replace("{{EmploymentType}}", newJobPostTemplate.EmploymentType);

                var mail = new EmailDataDto
                {
                    EmailToIds = [request.RequesterIdNavigation.Email],
                    EmailSubject = "Job Post Template Request",
                    EmailBody = emailTemplate
                };

                await _emailService.SendEmailAsync(mail);

                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Job post template request approved successfuly"
                };
            }
            else if (jobPostTemplateReview.Action == "Rejected")
            {
                // update request
                request.IsApproved = false;
                await _jobTemplateRequestRepository.UpdateAsync(request);

                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Job post template request rejected successfuly"
                };
            }
            else
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Request review error"
                };
            }
        }
        
        public async Task<JobPostTemplateRequestResponseDto> GetJobPostTemplateRequest(int id)
        {
            // Get the current logged-in user
            var userName = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.Name);

            // Get the user information from UserManager
            var user = await _userManager.FindByNameAsync(userName!);
            if (user == null)
            {
                return new JobPostTemplateRequestResponseDto
                {
                    Status = "Error",
                    Message = "User not found!"
                };
            }

            var jobPostTemplateRequest = await _jobTemplateRequestRepository.GetByIdAsync(id);

            if (jobPostTemplateRequest == null)
            {
                return new JobPostTemplateRequestResponseDto
                {
                    Status = "Error",
                    Message = "Job post template request is not found"
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

            var jobPostTemplateRequestDto = new JobPostTemplateRequestResponseDto
            {
                JobTitle = jobPostTemplateRequest.JobTitle,
                CompanyName = jobPostTemplateRequest.CompanyIdNavigation.Name,
                Description = jobPostTemplateRequest.Description,
                Requirements = jobPostTemplateRequest.Requirements,
                Location = jobPostTemplateRequest.Location,
                MinSalary = jobPostTemplateRequest.MinSalary,
                MaxSalary = jobPostTemplateRequest.MaxSalary,
                EmploymentType = jobPostTemplateRequest.EmploymentType,
                Status = "Success",
                Message = "Job post template request get successfuly"
            };
            return jobPostTemplateRequestDto;
        }
    }
}
