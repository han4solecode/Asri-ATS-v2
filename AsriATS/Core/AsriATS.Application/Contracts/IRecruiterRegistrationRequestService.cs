﻿using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.RecruiterRegistrationRequest;

namespace AsriATS.Application.Contracts
{
    public interface IRecruiterRegistrationRequestService
    {
        Task<BaseResponseDto> SubmitRecruiterRegistrationRequest(RecruiterRegistrationRequestDto request);
        Task<RecruiterRegistrationRequestResponseDto> ReviewRecruiterRegistrationRequest(int id);
    }
}