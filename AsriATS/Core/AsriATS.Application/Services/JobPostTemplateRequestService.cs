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
using System.Linq.Expressions;
using AsriATS.Application.DTOs.JobPost;
using AsriATS.Application.DTOs.Helpers;
using Org.BouncyCastle.Asn1.Ocsp;

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
            //var company = await _companyRepository.GetByIdAsync(request.CompanyId);
            //if (company == null)
            //{
            //    return new RegisterResponseDto
            //    {
            //        Status = "Error",
            //        Message = "Company is not registered!"
            //    };
            //}

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

            //if (user.CompanyId != request.CompanyId)
            //{
            //    return new BaseResponseDto
            //    {
            //        Status = "Error",
            //        Message = "You are not authorized to submit job post template for this company."
            //    };
            //}

            var newJobTemplateRequest = new JobPostTemplateRequest
            {
                JobTitle = request.JobTitle,
                CompanyId = user.CompanyId ?? 1,
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

        public async Task<object> GetAllJobPostTemplateRequest(JobPostSearch? jobPostSearch, Pagination? pagination)
        {
            //Get User based on user name
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);

            //Get User Roles based on role
            var userRoles = await _userManager.GetRolesAsync(user!);
            var userRole = userRoles.Single();

            Expression<Func<JobPostTemplateRequest, bool>>? expression = null;

            if (userRole == "Recruiter")
            {
                if(jobPostSearch.JobPostRequestStatus == "All Types")
                {
                    expression = (j => j.RequesterId == user!.Id);
                }
                //search pending job post template requests 
                else if (jobPostSearch?.JobPostRequestStatus == "Review HR Manager")
                {
                    expression = (j => j.RequesterId == user!.Id && j.IsApproved == null);
                }

                //search Approved job post template requests 
                else if (jobPostSearch.JobPostRequestStatus == "Accepted")
                {
                    expression = (j => j.RequesterId == user!.Id && j.IsApproved == true);

                }

                //search Rejected job post template requests 
                else if (jobPostSearch.JobPostRequestStatus == "Rejected")
                {
                    expression = (j => j.RequesterId == user!.Id && j.IsApproved == false);
                }
            }

            else if (userRole == "HR Manager")
            {
                if (jobPostSearch.JobPostRequestStatus == "All Types")
                {
                    expression = (j => j.CompanyId == user!.CompanyId && j.IsApproved != null);
                }
                //search to be reviewed job post template requests 
                else if (jobPostSearch?.JobPostRequestStatus == "Review HR Manager")
                {
                    expression = (j => j.CompanyId == user!.CompanyId && j.IsApproved == null);
                }

                //search Approved job post template requests 
                else if (jobPostSearch.JobPostRequestStatus == "Accepted")
                {
                    expression = (j => j.CompanyId == user!.CompanyId && j.IsApproved == true);

                }

                //search Rejected job post template requests 
                else if (jobPostSearch.JobPostRequestStatus == "Rejected")
                {
                    expression = (j => j.CompanyId == user!.CompanyId && j.IsApproved == false);
                }
            }

            var (jobPostTemplateRequests, totalCount) = await _jobTemplateRequestRepository.GetAllJobPostTemplateRequest(expression, jobPostSearch, pagination);

            var a = jobPostTemplateRequests.Select(j => new
            {
                JobPostTemplateRequestId = j.JobTemplateRequestId,
                Requester = $"{j.RequesterIdNavigation.FirstName} {j.RequesterIdNavigation.LastName}",
                JobTitle = j.JobTitle,
                Description = j.Description,
                Requirements = j.Requirements,
                Location = j.Location,
                MinSalary = j.MinSalary,
                MaxSalary = j.MaxSalary,
                EmploymentType = j.EmploymentType,
                Status = jobPostSearch.JobPostRequestStatus == "Accepted" ? "Accepted" : jobPostSearch.JobPostRequestStatus == "Rejected" ? "Rejected" : "Review by HR Manager",
            }).ToList();

            var totalPages = (int)Math.Ceiling((decimal)(totalCount) / (pagination.PageSize ?? 20));

            var result = new
            {
                Data = a,
                TotalPages = totalPages,
            };

            return result;
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
                JobTemplateRequestId = jobPostTemplateRequest.JobTemplateRequestId,
                JobTitle = jobPostTemplateRequest.JobTitle,
                CompanyName = jobPostTemplateRequest.CompanyIdNavigation.Name,
                Description = jobPostTemplateRequest.Description,
                Requirements = jobPostTemplateRequest.Requirements,
                Location = jobPostTemplateRequest.Location,
                MinSalary = jobPostTemplateRequest.MinSalary,
                MaxSalary = jobPostTemplateRequest.MaxSalary,
                EmploymentType = jobPostTemplateRequest.EmploymentType,
                IsApproved = jobPostTemplateRequest.IsApproved,
                Status = "Success",
                Message = "Job post template request get successfuly"
            };
            return jobPostTemplateRequestDto;
        }
    }
}
