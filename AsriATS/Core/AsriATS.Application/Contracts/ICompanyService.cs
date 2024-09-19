using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Company;

namespace AsriATS.Application.Contracts
{
    public interface ICompanyService
    {
        Task<BaseResponseDto> CompanyRegisterRequestAsync(CompanyRegisterRequestDto companyRegisterRequest);

        Task<BaseResponseDto> ReviewCompanyRegisterRequest(CompanyRegisterReviewDto companyRegisterReview);
    }
}