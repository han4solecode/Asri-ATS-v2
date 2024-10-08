﻿using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.InterivewScheduling;
using AsriATS.Application.DTOs.Request;
using AsriATS.Application.Services;
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

        [Authorize(Roles = "HR Manager")]
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

        [Authorize(Roles = "Applicant, HR Manager")]
        [HttpPost("review")]
        public async Task<IActionResult> ReviewInterviewProcess([FromBody] ReviewRequestDto reviewRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _interviewSchedulingService.ReviewInterviewProcess(reviewRequest);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }
        
        [Authorize(Roles = "HR Manager")]
        [HttpPut("update-schedule")]
        public async Task<IActionResult> UpdateInterviewSchedule([FromBody] UpdateInterviewScheduleDto updateInterview)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _interviewSchedulingService.UpdateInterviewSchedule(updateInterview);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);        
        }

        [Authorize(Roles = "Applicant")]
        [HttpPost("confirm")]
        public async Task<IActionResult> InterviewConfirmation([FromBody] ReviewRequestDto reviewRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _interviewSchedulingService.InterviewConfirmation(reviewRequest);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

        [Authorize(Roles = "HR Manager")]
        [HttpPost("mark-complete")]
        public async Task<IActionResult> MarkInterviewAsComplete([FromBody] MarkInterviewAsCompleteDto markInterviewAsComplete)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _interviewSchedulingService.MarkInterviewAsComplete(markInterviewAsComplete);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

        [Authorize(Roles = "HR Manager")]
        [HttpPost("review-result")]
        public async Task<IActionResult> ReviewInterviewResult([FromBody] ReviewRequestDto reviewRequest)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            var res = await _interviewSchedulingService.ReviewInterviewResult(reviewRequest);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }

        [Authorize]
        [HttpGet("unconfirmed-interview-schedule")]
        public async Task<IActionResult> GetUnconfirmedInterviewSchedulesAllAsync()
        {
            var result = await _interviewSchedulingService.GetAllUnconfirmedInterviewSchedules();
            return Ok(result);
        }

        [Authorize]
        [HttpGet("confirmed-interview-schedule")]
        public async Task<IActionResult> GetConfirmedInterviewSchedulesAllAsync()
        {
            var result = await _interviewSchedulingService.GetAllConfirmedInterviewSchedules();
            return Ok(result);
        }

        [Authorize(Roles = "HR Manager")]
        [HttpGet("completed-interview")]
        public async Task<IActionResult> GetAllCompletedInterview()
        {
            var result = await _interviewSchedulingService.GetAllCompletedInterview();

            return Ok(result);
        }
    }
}
