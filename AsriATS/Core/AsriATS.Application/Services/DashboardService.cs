using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Dashboard;
using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.Persistance;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AsriATS.Application.Services
{
    public class DashboardService : IDashboardService
    {
        private readonly IApplicationJobService _applicationJobService;
        private readonly IInterviewSchedulingService _interviewSchedulingService;
        private readonly IJobPostRequestService _jobPostRequestService;
        private readonly IRecruiterRegistrationRequestService _recruiterRegistrationRequestService;

        public DashboardService(IApplicationJobService applicationJobService, IInterviewSchedulingService interviewSchedulingService, IJobPostRequestService jobPostRequestService, IRecruiterRegistrationRequestService recruiterRegistrationRequestService)
        {
            _applicationJobService = applicationJobService;
            _interviewSchedulingService = interviewSchedulingService;
            _jobPostRequestService = jobPostRequestService;
            _recruiterRegistrationRequestService = recruiterRegistrationRequestService;
        }
        public async Task<ApplicantDashboardDto> GetApplicantDashboard()
        {
            var applicationPipeline = await _applicationJobService.ListAllApplicationStatuses();
            var notification = await _applicationJobService.NotificationApplicationStatuses();
            var pagination = new Pagination
            {
                PageNumber = null,
                PageSize = 10
            };
            var interview = await _interviewSchedulingService.GetAllUnconfirmedInterviewSchedules(pagination);

            return new ApplicantDashboardDto
            {
                ApplicationPipeline = applicationPipeline,
                Notification = notification,
                InterviewSchedule = interview.Data
            };
        }

        public async Task<RecruiterDashboardDto> GetRecruiterDashboard()
        {
            var application = await _applicationJobService.GetRecruiterDashboardMetricsAsync();
            var pipeline = await _applicationJobService.GetApplicationPipelineRecruiterAsync();
            var task = await _applicationJobService.ListAllApplicationStatuses();

            return new RecruiterDashboardDto
            {
                ApplicationPipeline = pipeline,
                AnalyticSnapshot = application,
                TaskReminders = task,
            };
        }

        public async Task<HRDashboardDto> GetHRManagerDashboard()
        {
            var jobPostRequestPagination = new Pagination { PageNumber = 1, PageSize = 5 };
            var jobpostRequests = await _jobPostRequestService.GetJobPostRequestToReview(null, jobPostRequestPagination);
            var recruiterRequests = await _recruiterRegistrationRequestService.GetAllRecruiterRegistrationRequests();

            recruiterRequests.ToBeReviewed.OrderByDescending(rr => rr.RecruiterRegistrationRequestId);
            recruiterRequests.ToBeReviewed.Take(5);
            
            return new HRDashboardDto
            {
                JobPostRequests = jobpostRequests,
                RecruiterRequests = recruiterRequests.ToBeReviewed,
            };
        }
    }
}
