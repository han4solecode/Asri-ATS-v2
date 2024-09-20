using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.RecruiterRegistrationRequest;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RecruiterRegistrationRequestController : ControllerBase
    {
        private readonly IRecruiterRegistrationRequestService _recruiterRegistrationRequestService;
        public RecruiterRegistrationRequestController(IRecruiterRegistrationRequestService recruiterRegistrationRequestService)
        {
            _recruiterRegistrationRequestService = recruiterRegistrationRequestService;
        }
        [HttpPost("create")]
        public async Task<IActionResult> CreateRecruiterRegistrationRequest([FromBody] RecruiterRegistrationRequestDto request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var res = await _recruiterRegistrationRequestService.SubmitRecruiterRegistrationRequest(request);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }

            return Ok(res);
        }
        // [Authorize(Roles = "HR Manager")]
        [HttpGet("{id}")]
        public async Task<IActionResult> ReviewRecruiterRegistrationRequest(int id)
        {
            var recruiterRegistrationRequest = await _recruiterRegistrationRequestService.ReviewRecruiterRegistrationRequest(id);

            if (recruiterRegistrationRequest.Status == "Error")
            {
                return NotFound(recruiterRegistrationRequest.Message);
            }
            return Ok(recruiterRegistrationRequest);
        }
        // [Authorize(Roles = "HR Manager")]
        [HttpPost("review-request/{id}")]
        public async Task<IActionResult> ApprovalRecruiterRegistrationRequest(int id, [FromBody] string action)
        {
            var res = await _recruiterRegistrationRequestService.ApprovalRecruiterRegistrationRequest(id, action);

            if (res.Status == "Error")
            {
                return BadRequest(res.Message);
            }
            return Ok(res);
        }
        // [Authorize(Roles = "HR Manager")]

        [HttpGet("recruiter-regist-request")]
        public async Task<IActionResult> GetAllRecruiterRegistrationRequest()
        {
            var cr = await _recruiterRegistrationRequestService.GetAllUnreviewedRecruiterRegistrationRequest();

            return Ok(cr);
        }
    }
}
