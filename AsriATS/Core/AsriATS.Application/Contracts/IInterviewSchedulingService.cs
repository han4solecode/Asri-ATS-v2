using AsriATS.Application.DTOs.InterivewScheduling;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Request;
using AsriATS.Domain.Entities;

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

        Task<IEnumerable<object>> GetAllUnconfirmedInterviewSchedules();

        Task<IEnumerable<object>> GetAllConfirmedInterviewSchedules();

        Task<IEnumerable<object>> GetAllCompletedInterview();
        Task<object ?> GetInterviewSchedule(int processId);
    }
}
