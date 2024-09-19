using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.ChangePassword;
using AsriATS.Application.DTOs.Login;
using AsriATS.Application.DTOs.RefreshToken;
using AsriATS.Application.DTOs.Register;
using AsriATS.Application.DTOs.Update;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.Contracts
{
    public interface IAuthService
    {
        Task<RegisterResponseDto> RegisterApplicantAsync(RegisterRequestDto register);

        Task<BaseResponseDto> RegisterHRManagerAsync(RegisterHRManagerRequestDto registerHRManagerRequest);

        Task<RefreshTokenResponseDto> RefreshAccessTokenAsync(string refreshToken);
        Task<LoginResponseDto> LoginAsync(LoginRequestDto login);
        Task<UpdateResponseDto> UpdateApplicantAsync(UpdateRequestDto update);
        Task<BaseResponseDto> ChangePasswordAsync(ChangePasswordRequestDto request);
    }
}
