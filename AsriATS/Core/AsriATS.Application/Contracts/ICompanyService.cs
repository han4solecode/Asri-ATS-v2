using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Company;
using AsriATS.Application.DTOs.Request;

namespace AsriATS.Application.Contracts
{
    public interface ICompanyService
    {
        Task<BaseResponseDto> CompanyRegisterRequestAsync(CompanyRegisterRequestDto companyRegisterRequest);

        Task<BaseResponseDto> ReviewCompanyRegisterRequest(ReviewRequestDto reviewRequest);
    }
}