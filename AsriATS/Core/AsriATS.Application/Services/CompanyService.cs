using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Company;
using AsriATS.Application.DTOs.Request;
using AsriATS.Application.Persistance;
using Microsoft.AspNetCore.Http;

namespace AsriATS.Application.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;
        private readonly IHttpContextAccessor _httpContextAccessor;

        public CompanyService(ICompanyRepository companyRepository, IHttpContextAccessor httpContextAccessor)
        {
            _companyRepository = companyRepository;
            _httpContextAccessor = httpContextAccessor;
        }

        public async Task<BaseResponseDto> CompanyRegisterRequestAsync(CompanyRegisterRequestDto companyRegisterRequest)
        {
            throw new NotImplementedException();
        }

        public async Task<BaseResponseDto> ReviewCompanyRegisterRequest(ReviewRequestDto reviewRequest)
        {
            throw new NotImplementedException();
        }
    }
}