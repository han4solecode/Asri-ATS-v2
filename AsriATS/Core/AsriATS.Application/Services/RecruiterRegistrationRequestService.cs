using AsriATS.Application.DTOs.RecruiterRegistrationRequest;
using AsriATS.Application.DTOs;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Register;
using Org.BouncyCastle.Asn1.Ocsp;

namespace AsriATS.Application.Services
{
    public class RecruiterRegistrationRequestService : IRecruiterRegistrationRequestService
    {
        private readonly IRecruiterRegistrationRequestRepository _recruiterRegistrationRequestRepository;
        private readonly ICompanyRepository _companyRepository;
        public RecruiterRegistrationRequestService(IRecruiterRegistrationRequestRepository recruiterRegistrationRequestRepository, ICompanyRepository companyRepository)
        {
            _recruiterRegistrationRequestRepository = recruiterRegistrationRequestRepository;
            _companyRepository = companyRepository;
        }
        public async Task<BaseResponseDto> SubmitRecruiterRegistrationRequest(RecruiterRegistrationRequestDto request)
        {
            var company = await _companyRepository.GetByIdAsync(request.CompanyId);
            if (company == null) 
            {
                return new RegisterResponseDto
                {
                    Status = "Error",
                    Message = "Company is not registered!"
                };
            }
            var newRecruiterRequest = new RecruiterRegistrationRequest
            {
                Email = request.Email,
                Name = request.Name,
                Address = request.Address,
                CompanyId = request.CompanyId,
                StartDate = DateTime.UtcNow
            };
            await _recruiterRegistrationRequestRepository.CreateAsync(newRecruiterRequest);
            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Recruiter Registration Request created successfully"
            };
        }

        public async Task<RecruiterRegistrationRequestResponseDto> ReviewRecruiterRegistrationRequest(int id)
        {
            var recruiterRegistrationRequest = await _recruiterRegistrationRequestRepository.GetByIdAsync(id);
            if (recruiterRegistrationRequest == null)
            {
                return new RecruiterRegistrationRequestResponseDto
                {
                    Status = "Error",
                    Message = "Recruiter Registration Request is not found"
                };
            }
            var recruiterRegistrationRequesResponseDto = new RecruiterRegistrationRequestResponseDto
            {
                Status = "Success",
                Message = "Recruiter Registration Request retrieved",
                Email = recruiterRegistrationRequest.Email,
                Name = recruiterRegistrationRequest.Name,
                Address = recruiterRegistrationRequest.Address,
                CompanyId = recruiterRegistrationRequest.CompanyId,
            };
            return recruiterRegistrationRequesResponseDto;
        }
    }
}
