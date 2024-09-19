using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Company;
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
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public CompanyService(ICompanyRepository companyRepository, IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _companyRepository = companyRepository;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _roleManager = roleManager;

        }

        public async Task<BaseResponseDto> CompanyRegisterRequestAsync(CompanyRegisterRequestDto companyRegisterRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponseDto> ReviewCompanyRegisterRequest(ReviewRequestDto reviewRequest)
        {
            throw new NotImplementedException();
        }

        public Task<BaseResponseDto> ReviewCompanyRegisterRequest(CompanyRegisterReviewDto companyRegisterReview)
        {
            throw new NotImplementedException();
        }
    }
}