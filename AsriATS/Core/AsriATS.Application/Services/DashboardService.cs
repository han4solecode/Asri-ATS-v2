﻿using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Dashboard;
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

        public DashboardService(IApplicationJobService applicationJobService, IInterviewSchedulingService interviewSchedulingService)
        {
            _applicationJobService = applicationJobService;
            _interviewSchedulingService = interviewSchedulingService;
        }
        public async Task<ApplicantDashboardDto> GetApplicantDashboard()
        {
            var applicationPipeline = await _applicationJobService.ListAllApplicationStatuses();
            var interview = await _interviewSchedulingService.GetAllUnconfirmedInterviewSchedules();

            return new ApplicantDashboardDto
            {
                ApplicationPipeline = applicationPipeline,
                InterviewSchedule = interview
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
    }
}
