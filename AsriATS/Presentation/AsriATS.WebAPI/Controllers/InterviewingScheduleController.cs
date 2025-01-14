using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.Helpers;
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
        /// <summary>
        /// Sets the interview schedule for an applicant.
        /// </summary>
        /// <remarks>
        /// All parameters in the request body must be provided and cannot be null.
        /// 
        /// Note: This operation requires authorization via a bearer token, and is only accessible to users with the "HR Manager" role. 
        /// Additionally, only HR Managers from the company associated with the application can set the interview schedule.
        /// 
        /// Sample request:
        /// 
        ///     POST https://localhost:7080/api/InterviewingSchedule/SetInterviewSchedule
        ///     {
        ///         "ApplicationJobId" : 3,
        ///         "InterviewTime": "2024-10-03T14:30:00+07:00",
        ///         "Interviewers":["Dayat", "Steven"],
        ///         "InterviewType":"Offline",
        ///         "Action" :"Submit",
        ///         "Comment" :"Potential candidate",
        ///         "InterviewerEmails":["leusulappaubrou-1701@yopmail.com"],
        ///         "Location": "Test1 office building"
        ///     }
        ///     
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
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

            return Ok(result);
        }
        /// <summary>
        /// Updates the interview schedule for an applicant.
        /// </summary>
        /// <remarks>
        /// All parameters in the request body must be provided and cannot be null, except for the "Comment" field, which is optional.
        /// 
        /// Note: This operation requires authorization via a bearer token and is only accessible to users with the "HR Manager" role. 
        /// Additionally, only HR Managers from the company associated with the application can update the interview schedule.
        /// 
        /// Sample request:
        /// 
        ///     PUT https://localhost:7080/api/InterviewingSchedule/update-schedule
        ///     {
        ///         "ProcessId": 1,
        ///         "InterviewTime": "2024-10-03T14:30:00+07:00",
        ///         "Comment": "Potential candidate"
        ///     }
        /// 
        /// </remarks>
        /// <param name="updateInterview"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Confirms the interview schedule for an applicant.
        /// </summary>
        /// <remarks>
        /// All parameters in the request body must be provided and cannot be null, except for the "Comment" field, which is optional.
        /// 
        /// Note: This operation requires authorization via a bearer token and is only accessible to users with the "Applicant" role.
        /// 
        /// Sample request:
        /// 
        ///     POST https://localhost:7080/api/InterviewingSchedule/confirm
        ///     {
        ///         "ProcessId": 1,
        ///         "Action": "Confirm",
        ///         "Comment": ""
        ///     }
        ///     
        /// </remarks>
        /// <param name="reviewRequest"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Updates to mark the interview process to complete.
        /// </summary>
        /// <remarks>
        /// All parameters in the request body must be provided and cannot be null.
        /// 
        /// Note: This operation requires authorization via a bearer token, and is only accessible to users with the "HR Manager" role. 
        /// Additionally, only HR Managers from the company associated with the application can mark the interview process to complete.
        /// 
        /// Sample request:
        /// 
        ///     POST https://localhost:7080/api/InterviewingSchedule/mark-complete
        ///     {
        ///         "ProcessId": 1,
        ///         "InterviewersComments": ["excellent", "we are looking for this candidate"],
        ///         "Comment" :"Interview is done",
        ///     }
        ///     
        /// </remarks>
        /// <param name="markInterviewAsComplete"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Reviews the interview result and extends an offer to the applicant if deemed suitable.
        /// </summary>
        /// <remarks>
        /// All parameters in the request body must be provided and cannot be null.
        /// 
        /// Note: This operation requires authorization via a bearer token and is only accessible to users with the "HR Manager" role. 
        /// Additionally, only HR Managers from the company associated with the application can mark the interview process as complete.
        /// 
        /// Sample request:
        /// 
        ///     POST https://localhost:7080/api/InterviewingSchedule/review-result
        ///     {
        ///         "ProcessId": 1,
        ///         "Action": "Offer",
        ///         "Comment": "Offering has been sent to this candidate."
        ///     }
        /// 
        /// </remarks>
        /// <param name="reviewRequest"></param>
        /// <returns></returns>
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
        /// <summary>
        /// Retrieves all unconfirmed interview schedules and modified interview schedule requests.
        /// </summary>
        /// <remarks>
        /// This API endpoint retrieves all unconfirmed interview schedules as well as modified interview schedule requests.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/InterviewingSchedule/unconfirmed-interview-schedule
        /// 
        /// The response includes a JSON object containing all interview schedules that have not been confirmed.
        /// </remarks>
        /// <returns>Returns all interview schedules that have not been confirmed.</returns>
        [Authorize]
        [HttpGet("unconfirmed-interview-schedule")]
        public async Task<IActionResult> GetUnconfirmedInterviewSchedulesAllAsync([FromQuery] Pagination pagination)
        {
            var result = await _interviewSchedulingService.GetAllUnconfirmedInterviewSchedules(pagination);
            return Ok(result);
        }
        /// <summary>
        /// Retrieves all confirmed interview schedules.
        /// </summary>
        /// <remarks>
        /// This API endpoint retrieves all confirmed interview schedules.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/InterviewingSchedule/confirmed-interview-schedule
        /// 
        /// The response includes a JSON object containing all interview schedules that have been confirmed.
        /// </remarks>
        /// <returns>Returns all interview schedules that have been confirmed.</returns>
        [Authorize]
        [HttpGet("confirmed-interview-schedule")]
        public async Task<IActionResult> GetConfirmedInterviewSchedulesAllAsync([FromQuery] Pagination pagination)
        {
            var result = await _interviewSchedulingService.GetAllConfirmedInterviewSchedules(pagination);
            return Ok(result);
        }
        /// <summary>
        /// Retrieves all completed interview schedules.
        /// </summary>
        /// <remarks>
        /// This API endpoint retrieves all completed interview schedules.
        /// 
        /// Note: This operation requires authorization via a bearer token and is only accessible to users with the "HR Manager" role. 
        /// Additionally, only HR Managers from the company specified in the request are allowed to view completed interview schedules.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/InterviewingSchedule/completed-interview
        /// 
        /// The response includes a JSON object containing all interview schedules that have been completed.
        /// </remarks>
        /// <returns>Returns all interview schedules that have been completed.</returns>
        [Authorize(Roles = "Applicant,HR Manager,Recruiter")]
        [HttpGet("completed-interview")]
        public async Task<IActionResult> GetAllCompletedInterview([FromQuery] Pagination pagination)
        {
            var result = await _interviewSchedulingService.GetAllCompletedInterview(pagination);

            return Ok(result);
        }

        [Authorize]
        [HttpGet("application/{processId}")]
        public async Task<IActionResult> GetInterviewSchedule(int processId)
        {
            var res = await _interviewSchedulingService.GetInterviewSchedule(processId);
            return Ok(res);
        }
    }
}
