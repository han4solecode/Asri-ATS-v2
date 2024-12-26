using AsriATS.Application.DTOs.ApplicationJob;
using AsriATS.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AsriATS.Application.DTOs.Request;

namespace AsriATS.Application.Contracts
{
    public interface IApplicationJobService
    {
        Task<BaseResponseDto> SubmitApplicationJob(ApplicationJobDto request, List<IFormFile> supportingDocuments);
        Task<IEnumerable<object>> GetAllApplicationStatuses();

        Task<SupportingDocumentResponseDto> GetAllSupportingDocuments();

        Task<SupportingDocumentResponseDto> GetSupportingDocumentById(int id);

        Task<IEnumerable<object>> GetAllIncomingApplications();

        Task<BaseResponseDto> ReviewJobApplication(ReviewRequestDto reviewRequest);
        Task<BaseResponseDto> UpdateApplicationJob(UpdateApplicationJobDto requestDto, List<IFormFile>? supportingDocuments = null);
        Task<ApplicationDetailDto> GetProcessAsync(int processId);
    }
}
