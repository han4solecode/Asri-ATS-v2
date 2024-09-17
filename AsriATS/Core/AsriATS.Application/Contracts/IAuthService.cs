using AsriATS.Application.DTOs.Register;
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
    }
}
