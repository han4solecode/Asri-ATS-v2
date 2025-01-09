using AsriATS.Application.DTOs.RecruiterRegistrationRequest;
using AsriATS.Application.DTOs;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Register;
using Microsoft.AspNetCore.Identity;
using AsriATS.Application.DTOs.Email;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Diagnostics;
using System.ComponentModel.Design;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace AsriATS.Application.Services
{
    public class RecruiterRegistrationRequestService : IRecruiterRegistrationRequestService
    {
        private readonly IRecruiterRegistrationRequestRepository _recruiterRegistrationRequestRepository;
        private readonly ICompanyRepository _companyRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly IEmailService _emailService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private static readonly Random Random = new Random();
        public RecruiterRegistrationRequestService(IRecruiterRegistrationRequestRepository recruiterRegistrationRequestRepository, ICompanyRepository companyRepository, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, IEmailService emailService, IHttpContextAccessor httpContextAccessor)
        {
            _recruiterRegistrationRequestRepository = recruiterRegistrationRequestRepository;
            _companyRepository = companyRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _emailService = emailService;
            _httpContextAccessor = httpContextAccessor;
        }
        public async Task<BaseResponseDto> SubmitRecruiterRegistrationRequest(RecruiterRegistrationRequestDto request)
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

            var userExist = await _userManager.FindByEmailAsync(request.Email);

            if (userExist != null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "User already exist!"
                };
            }
            var emailRequestExist = await _recruiterRegistrationRequestRepository.FindByEmailAsync(request.Email);

            if (emailRequestExist != null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Email has been used for request before"
                };
            }
            var newRecruiterRequest = new RecruiterRegistrationRequest
            {
                PhoneNumber = request.PhoneNumber,
                FirstName = request.FirstName,
                LastName = request.LastName,
                Dob = request.Dob,
                Sex = request.Sex,
                Email = request.Email,
                Address = request.Address,
                CompanyId = request.CompanyId,
                StartDate = DateTime.UtcNow
            };
            await _recruiterRegistrationRequestRepository.CreateAsync(newRecruiterRequest);
            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Recruiter Registration Request created successfully"
            };
        }

        public async Task<RecruiterRegistrationRequestResponseDto> ReviewRecruiterRegistrationRequest(int id)
        {
            var recruiterRegistrationRequest = await _recruiterRegistrationRequestRepository
                .GetAll().Include(r => r.CompanyIdNavigation).FirstOrDefaultAsync(r => r.RecruiterRegistrationRequestId == id);

            if (recruiterRegistrationRequest == null)
            {
                return new RecruiterRegistrationRequestResponseDto
                {
                    Status = "Error",
                    Message = "Recruiter Registration Request is not found"
                };
            }
            var recruiterRegistrationRequesResponseDto = new RecruiterRegistrationRequestResponseDto
            {
                Status = "Success",
                Message = "Recruiter Registration Request retrieved",
                Email = recruiterRegistrationRequest.Email,
                FirstName = recruiterRegistrationRequest.FirstName,
                LastName = recruiterRegistrationRequest.LastName,
                PhoneNumber = recruiterRegistrationRequest.PhoneNumber,
                Dob = recruiterRegistrationRequest.Dob,
                Sex = recruiterRegistrationRequest.Sex,
                Address = recruiterRegistrationRequest.Address,
                CompanyName = recruiterRegistrationRequest.CompanyIdNavigation.Name ?? "No Company Name",
                IsApproved = recruiterRegistrationRequest.IsApproved,
            };
            return recruiterRegistrationRequesResponseDto;
        }
        public async Task<BaseResponseDto> ApprovalRecruiterRegistrationRequest(int id, string action)
        {
            var recruiterRequest = await _recruiterRegistrationRequestRepository.GetByIdAsync(id);
            if (recruiterRequest == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Recruiter Registration Request is not found"
                };
            }
            if (recruiterRequest.IsApproved != null)
            {
                return new RecruiterRegistrationRequestResponseDto
                {
                    Status = "Error",
                    Message = "Recruiter Registration Request has been processed before"
                };
            }
            if (action == "Approved")
            {
                recruiterRequest.IsApproved = true;
                recruiterRequest.EndDate = DateTime.UtcNow;
                await _recruiterRegistrationRequestRepository.UpdateAsync(recruiterRequest);

                var newUser = new AppUser
                {
                    UserName = recruiterRequest.Email,
                    Email = recruiterRequest.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    PhoneNumber = recruiterRequest.PhoneNumber,
                    FirstName = recruiterRequest.FirstName,
                    LastName = recruiterRequest.LastName,
                    Address = recruiterRequest.Address,
                    Dob = recruiterRequest.Dob,
                    Sex = recruiterRequest.Sex,
                    CompanyId = recruiterRequest.CompanyId,
                };

                string password = GeneratePassword();

                var result = await _userManager.CreateAsync(newUser, password);

                if (!result.Succeeded)
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "User registration failed! Please check user details and try again."
                    };
                }

                if (await _roleManager.RoleExistsAsync("Recruiter"))
                {
                    await _userManager.AddToRoleAsync(newUser, "Recruiter");
                }

                // send email notification to new user to give the newly created credential (username and password)
                var emailTemplate = File.ReadAllText(@"./Templates/EmailTemplates/CredentialForNewRegisteredRecruiter.html");

                emailTemplate = emailTemplate.Replace("{{Name}}", $"{recruiterRequest.FirstName} {recruiterRequest.LastName}");
                emailTemplate = emailTemplate.Replace("{{UserName}}", recruiterRequest.Email);
                emailTemplate = emailTemplate.Replace("{{Password}}", password);

                var mail = new EmailDataDto
                {
                    EmailToIds = [recruiterRequest.Email],
                    EmailSubject = "Registration Approved",
                    EmailBody = emailTemplate
                };

                await _emailService.SendEmailAsync(mail);

                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Recruiter Registration Request approved successfully"
                };
            }
            else if (action == "Rejected")
            {
                recruiterRequest.IsApproved = false;
                recruiterRequest.EndDate = DateTime.UtcNow;
                await _recruiterRegistrationRequestRepository.UpdateAsync(recruiterRequest);

                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Recruiter Registration Request rejected successfully"
                };
            }
            return new BaseResponseDto
            {
                Status = "Error",
                Message = "Recruiter Registration Approval request is error"
            };
        }

        public async Task<RecruiterRegistrationListDto> GetAllRecruiterRegistrationRequests()
        {
            // Get the currently logged-in user
            var userName = _httpContextAccessor.HttpContext?.User.Identity?.Name;

            if (string.IsNullOrEmpty(userName))
            {
                throw new UnauthorizedAccessException("User is not authenticated.");
            }

            // Retrieve the user details from the UserManager
            var user = await _userManager.FindByNameAsync(userName);

            if (user == null)
            {
                throw new UnauthorizedAccessException("User not found.");
            }

            // Ensure the user has a CompanyId
            if (!user.CompanyId.HasValue)
            {
                throw new InvalidOperationException("User does not have a company associated.");
            }

            var userCompanyId = user.CompanyId.Value;

            // Fetch recruiter registration requests that are to be reviewed
            var rrToBeReviewed = await _recruiterRegistrationRequestRepository
                .GetAllToBeReviewedAsync(r => r.CompanyId == userCompanyId && r.IsApproved == null);

            var rrToBeReviewedDto = rrToBeReviewed.Select(rr => new AllRecruiterRegistrationRequestDto
            {
                RecruiterRegistrationRequestId = rr.RecruiterRegistrationRequestId,
                Email = rr.Email,
                FirstName = rr.FirstName,
                LastName = rr.LastName,
                PhoneNumber = rr.PhoneNumber,
                Dob = rr.Dob,
                Sex = rr.Sex,
                Address = rr.Address,
                CompanyName = rr.CompanyIdNavigation.Name ?? "No company Name",
                IsApproved = rr.IsApproved == null, // Still to be reviewed
            }).ToList();

            // Fetch recruiter registration requests that are already reviewed
            var rrAlreadyReviewed = await _recruiterRegistrationRequestRepository
                .GetAllToBeReviewedAsync(r => r.CompanyId == userCompanyId && r.IsApproved != null);

            var rrAlreadyReviewedDto = rrAlreadyReviewed.Select(rr => new AllRecruiterRegistrationRequestDto
            {
                RecruiterRegistrationRequestId = rr.RecruiterRegistrationRequestId,
                Email = rr.Email,
                FirstName = rr.FirstName,
                LastName = rr.LastName,
                PhoneNumber = rr.PhoneNumber,
                Dob = rr.Dob,
                Sex = rr.Sex,
                Address = rr.Address,
                CompanyName = rr.CompanyIdNavigation.Name ?? "No company Name",
                IsApproved = rr.IsApproved != null, // Already reviewed
            }).ToList();

            // Return both lists encapsulated in a response DTO
            return new RecruiterRegistrationListDto
            {
                ToBeReviewed = rrToBeReviewedDto,
                AlreadyReviewed = rrAlreadyReviewedDto
            };
        }


        private static string GeneratePassword(int length = 8)
        {
            if (length < 8)
            {
                throw new ArgumentException("Password must be at least 8 characters long.");
            }

            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";

            char[] password = new char[length];
            password[0] = upperCase[Random.Next(upperCase.Length)];
            password[1] = lowerCase[Random.Next(lowerCase.Length)];
            password[2] = digits[Random.Next(digits.Length)];

            string allChars = upperCase + lowerCase + digits;
            for (int i = 3; i < length; i++)
            {
                password[i] = allChars[Random.Next(allChars.Length)];
            }

            return new string(password.OrderBy(_ => Random.Next()).ToArray());
        }
    }
}
