using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Company;
using AsriATS.Application.DTOs.Request;
using AsriATS.Application.Persistance;

namespace AsriATS.Application.Services
{
    public class CompanyService : ICompanyService
    {
        private readonly ICompanyRepository _companyRepository;

        public CompanyService(ICompanyRepository companyRepository)
        {
            _companyRepository = companyRepository;
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