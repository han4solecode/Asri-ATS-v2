using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Company;
using AsriATS.Domain.Entities;

namespace AsriATS.Application.Contracts
{
    public interface ICompanyService
    {
        Task<BaseResponseDto> CompanyRegisterRequestAsync(CompanyRegisterRequestDto companyRegisterRequest);

        Task<BaseResponseDto> ReviewCompanyRegisterRequest(CompanyRegisterReviewDto companyRegisterReview);

        Task<IEnumerable<CompanyRequest>> GetAllCompanyRegisterRequest();
    }
}