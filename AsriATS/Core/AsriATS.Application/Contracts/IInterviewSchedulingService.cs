using AsriATS.Application.DTOs.InterivewScheduling;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Request;
using AsriATS.Domain.Entities;
using AsriATS.Application.DTOs.Helpers;
using AsriATS.Application.DTOs.Dashboard;

namespace AsriATS.Application.Contracts
{
    public interface IInterviewSchedulingService
    {
        Task<BaseResponseDto> SetInterviewSchedule(InterviewSchedulingRequestDto request);

        Task<BaseResponseDto> ReviewInterviewProcess(ReviewRequestDto reviewRequest);

        Task<BaseResponseDto> UpdateInterviewSchedule(UpdateInterviewScheduleDto updateInterview);

        Task<BaseResponseDto> InterviewConfirmation(ReviewRequestDto reviewRequest);

        Task<BaseResponseDto> MarkInterviewAsComplete(MarkInterviewAsCompleteDto markInterviewAsComplete);

        Task<BaseResponseDto> ReviewInterviewResult(ReviewRequestDto reviewRequest);

        Task<InterviewResponseDashboardDto> GetAllUnconfirmedInterviewSchedules(Pagination? pagination);

        Task<object> GetAllConfirmedInterviewSchedules(Pagination? pagination);

        Task<object> GetAllCompletedInterview(Pagination? pagination);
        Task<object ?> GetInterviewSchedule(int processId);
    }
}
