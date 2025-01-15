using AsriATS.Application.DTOs.Dashboard;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.Contracts
{
    public interface IDashboardService
    {
        public Task<ApplicantDashboardDto> GetApplicantDashboard();
        Task<RecruiterDashboardDto> GetRecruiterDashboard();
        Task<HRDashboardDto> GetHRManagerDashboard();
    }
}
