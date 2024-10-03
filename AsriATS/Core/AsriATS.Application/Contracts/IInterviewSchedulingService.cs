using AsriATS.Application.DTOs.InterivewScheduling;
using AsriATS.Application.DTOs;

namespace AsriATS.Application.Contracts
{
    public interface IInterviewSchedulingService
    {
        Task<BaseResponseDto> SetInterviewSchedule(InterviewSchedulingRequestDto request);
    }
}
