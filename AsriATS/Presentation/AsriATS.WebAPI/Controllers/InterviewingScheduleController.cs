using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.InterivewScheduling;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class InterviewingScheduleController : ControllerBase
    {
        private readonly IInterviewSchedulingService _interviewSchedulingService;

        public InterviewingScheduleController(IInterviewSchedulingService interviewSchedulingService)
        {
            _interviewSchedulingService = interviewSchedulingService;
        }

        [Authorize(Roles = "HR Manager, Recruiter")]
        [HttpPost("SetInterviewSchedule")]
        public async Task<IActionResult> SetInterviewSchedule([FromBody] InterviewSchedulingRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            // Call the service method with both arguments
            var result = await _interviewSchedulingService.SetInterviewSchedule(request);

            if (result.Status == "Error")
            {
                return BadRequest(new { message = result.Message });
            }

            return Ok(new { message = result.Message });
        }
    }
}
