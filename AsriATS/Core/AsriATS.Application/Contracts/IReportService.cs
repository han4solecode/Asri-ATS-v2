using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.Contracts
{
    public interface IReportService
    {
        Task<byte[]> GenerateOverallRecruitmentMetricsAsync();
        Task<byte[]> GenerateDemographicReportAsync(string address);
        Task<byte[]> GenerateRecruitmentFunnelReportAsync();
    }
}
