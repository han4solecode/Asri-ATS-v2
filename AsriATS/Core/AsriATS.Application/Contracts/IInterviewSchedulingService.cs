using AsriATS.Application.DTOs.InterivewScheduling;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Request;

namespace AsriATS.Application.Contracts
{
    public interface IInterviewSchedulingService
    {
        Task<BaseResponseDto> SetInterviewSchedule(InterviewSchedulingRequestDto request);

        Task<BaseResponseDto> ReviewInterviewProcess(ReviewRequestDto reviewRequest);

        Task<BaseResponseDto> UpdateInterviewSchedule(UpdateInterviewScheduleDto updateInterview);

        Task<BaseResponseDto> InterviewConfirmation(ReviewRequestDto reviewRequest);

        Task<IEnumerable<object>> GetAllInterviewSchedules();
    }
}
