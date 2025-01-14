using AsriATS.Application.DTOs.ApplicationJob;
using AsriATS.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using AsriATS.Application.DTOs.Request;
using AsriATS.Application.DTOs.Helpers;

namespace AsriATS.Application.Contracts
{
    public interface IApplicationJobService
    {
        Task<BaseResponseDto> SubmitApplicationJob(ApplicationJobDto request, List<IFormFile> supportingDocuments);
        Task<object> GetApplicationStatusesAsync(ApplicationJobSearchDto searchParams);
        Task<SupportingDocumentResponseDto> GetAllSupportingDocuments();

        Task<SupportingDocumentResponseDto> GetSupportingDocumentById(int id);

        Task<IEnumerable<object>> GetAllIncomingApplications();

        Task<BaseResponseDto> ReviewJobApplication(ReviewRequestDto reviewRequest);
        Task<BaseResponseDto> UpdateApplicationJob(UpdateApplicationJobDto requestDto, List<IFormFile>? supportingDocuments = null);
        Task<ApplicationDetailDto> GetProcessAsync(int processId);
        Task<object> ListAllApplicationStatuses();
        Task<object> GetRecruiterDashboardMetricsAsync();
        Task<object> GetApplicationPipelineRecruiterAsync();
        Task<object> NotificationApplicationStatuses();
    }
}
