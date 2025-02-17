using System.Security.Cryptography;
using System.Text;
using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Company;
using AsriATS.Application.DTOs.Email;
using AsriATS.Application.DTOs.Request;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AsriATS.Application.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ICompanyRequestRepository _companyRequestRepository;
        private readonly IEmailService _emailService;

        public CompanyService(ICompanyRepository companyRepository, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager, ICompanyRequestRepository companyRequestRepository, IEmailService emailService)
        {
            _companyRepository = companyRepository;
            _userManager = userManager;
            _roleManager = roleManager;
            _companyRequestRepository = companyRequestRepository;
            _emailService = emailService;
        }

        public async Task<BaseResponseDto> CompanyRegisterRequestAsync(CompanyRegisterRequestDto companyRegisterRequest)
        {
            try
            {
                var newCompanyRequest = new CompanyRequest
                {
                    CompanyAddress = companyRegisterRequest.CompanyAddress,
                    CompanyName = companyRegisterRequest.CompanyName,
                    Email = companyRegisterRequest.Email,
                    FirstName = companyRegisterRequest.FirstName,
                    LastName = companyRegisterRequest.LastName,
                    UserAddress = companyRegisterRequest.UserAddress,
                    Dob = companyRegisterRequest.Dob,
                    Sex = companyRegisterRequest.Sex,
                    PhoneNumber = companyRegisterRequest.PhoneNumber
                };

                var cr = await _companyRequestRepository.GetAllAsync();

                if (cr.Any(x => x.Email == newCompanyRequest.Email || x.CompanyName == newCompanyRequest.CompanyName))
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "Email or Company name already in use"
                    };
                }

                await _companyRequestRepository.CreateAsync(newCompanyRequest);

                // send email notification to companyRegisterRequest.Email


                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Company Registered Successfuly"
                };
            }
            catch (System.Exception)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Company Registration Error"
                };
            }
        }

        public async Task<BaseResponseDto> ReviewCompanyRegisterRequest(CompanyRegisterReviewDto companyRegisterReview)
        {
            // if approved
            // set isApproved = true
            // var newCompanny = new Company { name address }
            // create new HR manager user (username = Email, password = generate random, CompanyId = 1, FirstName = FirstName)
            // send email
            // if rejected
            // do nothing (send email)

            var cr = await _companyRequestRepository.GetByIdAsync(companyRegisterReview.CompanyRequestId);

            if (cr == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Company request does not exist"
                };
            }

            if (companyRegisterReview.Action == "Approved")
            {
                // update cr
                cr.IsApproved = true;
                await _companyRequestRepository.UpdateAsync(cr);

                // create new company
                var newCompany = new Company
                {
                    Name = cr.CompanyName,
                    Address = cr.CompanyAddress,
                };

                await _companyRepository.CreateAsync(newCompany);

                // create new HR Manager with newly created companyId
                var userExist = await _userManager.FindByNameAsync(cr.Email);

                if (userExist != null)
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "User already exist!"
                    };
                }

                var newUser = new AppUser
                {
                    UserName = cr.Email,
                    Email = cr.Email,
                    SecurityStamp = Guid.NewGuid().ToString(),
                    PhoneNumber = cr.PhoneNumber,
                    FirstName = cr.FirstName,
                    LastName = cr.LastName,
                    Address = cr.UserAddress,
                    Dob = cr.Dob,
                    Sex = cr.Sex,
                    CompanyId = newCompany.CompanyId
                };

                var password = GeneratePassword(12);

                var result = await _userManager.CreateAsync(newUser, password);

                if (!result.Succeeded)
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "User registration failed! Please check user details and try again."
                    };
                }

                if (await _roleManager.RoleExistsAsync("HR Manager"))
                {
                    await _userManager.AddToRoleAsync(newUser, "HR Manager");
                }

                // send email notification to new user to give the newly created credential (username and password)
                var emailTemplate = File.ReadAllText(@"./Templates/EmailTemplates/CompanyRegistrationApproved.html");

                // email template replace text with necessary data
                emailTemplate = emailTemplate.Replace("{{Name}}", $"{newUser.FirstName} {newUser.LastName}");
                emailTemplate = emailTemplate.Replace("{{CompanyName}}", newCompany.Name);
                emailTemplate = emailTemplate.Replace("{{UserName}}", newUser.UserName);
                emailTemplate = emailTemplate.Replace("{{Password}}", password);

                var mail = new EmailDataDto
                {
                    EmailToIds = [newUser.Email],
                    EmailSubject = "Asri ATS Company Registration",
                    EmailBody = emailTemplate
                };

                await _emailService.SendEmailAsync(mail);

                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Company registration request approved successfuly"
                };
            }
            else
            {
                // update cr
                cr.IsApproved = false;
                await _companyRequestRepository.UpdateAsync(cr);

                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Company registration request rejected successfuly"
                };
            }
        }

        public async Task<IEnumerable<CompanyRequest>> GetAllCompanyRegisterRequest()
        {
            var crToBeReviewed = await _companyRequestRepository.GetAllToBeReviewedAsync();

            return crToBeReviewed;
        }

        private static string GeneratePassword(int length)
        {
            // char[] chars = "abcdefghijklmnopqrstuvwxyzABCDEFGHIJKLMNOPQRSTUVWXYZ1234567890".ToCharArray();

            // byte[] data = new byte[4 * length];
            // using (var crypto = RandomNumberGenerator.Create())
            // {
            //     crypto.GetBytes(data);
            // }
            // StringBuilder result = new(length);

            // for (int i = 0; i < length; i++)
            // {
            //     var rnd = BitConverter.ToUInt32(data, i * 4);
            //     var idx = rnd % chars.Length;

            //     result.Append(chars[idx]);
            // }

            // return result.ToString();

            const string upperCase = "ABCDEFGHIJKLMNOPQRSTUVWXYZ";
            const string lowerCase = "abcdefghijklmnopqrstuvwxyz";
            const string digits = "0123456789";

            char[] password = new char[length];
            password[0] = upperCase[new Random().Next(upperCase.Length)];
            password[1] = lowerCase[new Random().Next(lowerCase.Length)];
            password[2] = digits[new Random().Next(digits.Length)];

            string allChars = upperCase + lowerCase + digits;
            for (int i = 3; i < length; i++)
            {
                password[i] = allChars[new Random().Next(allChars.Length)];
            }

            return new string([.. password.OrderBy(_ => new Random().Next())]);
        }

        public async Task<IEnumerable<Company>> GetAllCompany()
        {
            var company = await _companyRepository.GetAllAsync();

            return company;
        }

        public async Task<CompanyRequest?> GetCompanyRegistrationRequestById(int id)
        {
            var companynRegistrationRequest = await _companyRequestRepository.GetByIdAsync(id);

            return companynRegistrationRequest;
        }
    }
}