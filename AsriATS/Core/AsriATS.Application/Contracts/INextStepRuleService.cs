using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.NextStepRule;

namespace AsriATS.Application.Contracts
{
    public interface INextStepRuleService
    {
        Task<BaseResponseDto> CreateNextStepRuleAsync(NextStepRuleRequestDto request);
    }
}
