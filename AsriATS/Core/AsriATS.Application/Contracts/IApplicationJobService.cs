using AsriATS.Application.DTOs.ApplicationJob;
using AsriATS.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;

namespace AsriATS.Application.Contracts
{
    public interface IApplicationJobService
    {
        Task<BaseResponseDto> SubmitApplicationJob(ApplicationJobDto request, List<IFormFile> supportingDocuments);
        Task<IEnumerable<object>> GetAllApplicationStatuses();

        Task<IEnumerable<object>> GetAllSupportingDocuments();
    }
}
