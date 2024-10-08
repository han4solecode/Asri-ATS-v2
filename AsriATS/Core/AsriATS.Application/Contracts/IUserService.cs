using AsriATS.Application.DTOs.ChangePassword;
using AsriATS.Application.DTOs.Update;
using AsriATS.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AsriATS.Application.DTOs.User;
using AsriATS.Application.DTOs.Report;
using Microsoft.AspNetCore.Http;

namespace AsriATS.Application.Contracts
{
    public interface IUserService
    {
        Task<UpdateResponseDto> UpdateUserAsync(UpdateRequestDto update);
        Task<object> GetUserInfo();
        Task<bool> DeleteUserAsync(string userName);
        Task<List<UserResponseDto>> GetUsersInSameCompanyAsync();
        Task<IEnumerable<object>> GetAllUserInfoAsync();

        Task<BaseResponseDto> UploadDocumentAsync(IFormFile file);

        Task<BaseResponseDto> DeleteDocumentAsync(int id);
        Task<IEnumerable<DemographicOverviewDto>> GetDemographicOverviewAsync(string address);
    }
}
