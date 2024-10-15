using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs.RecruiterRegistrationRequest;
using Microsoft.AspNetCore.Authorization;
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

        /// <summary>
        /// Create a recruiter registration request
        /// </summary>
        /// <remarks>
        /// Sample request:
        /// 
        ///     POST /api/RecruiterRegistrationRequest/create
        ///     {
        ///         "email": "larrymusk@gmail.com",
        ///         "address": "Jl. Kesepian",
        ///         "companyId": 2,
        ///         "firstName": "Larry",
        ///         "lastName": "Musk",
        ///         "dob": "1998-01-09",
        ///         "sex": "Male",
        ///         "phoneNumber": "08114455676"
        ///     }
        /// 
        /// </remarks>
        /// <param name="request"></param>
        /// <returns></returns>
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

        /// <summary>
        /// Retrieve recruiter registration request by id
        /// </summary>
        /// <param name="id"></param>
        /// <remarks>
        /// Only user with the role "HR Manager" is authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     GET /api/RecruiterRegistrationRequest/{id}
        /// 
        /// </remarks>
        /// <returns>A recruiter registration request data</returns>
        [Authorize(Roles = "HR Manager")]
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

        /// <summary>
        /// Approve or reject a recruiter registration request
        /// </summary>
        /// <remarks>
        /// Only user with the role "HR Manager" is authorized to access this endpoint.
        /// Action must be "Approved" or "Rejected".
        /// 
        /// Sample request:
        /// 
        ///     POST /api/RecruiterRegistrationRequest/review-request/{id}
        ///     {
        ///         "Approved"
        ///     }
        /// 
        /// </remarks>
        /// <param name="id"></param>
        /// <param name="action"></param>
        /// <returns></returns>
        [Authorize(Roles = "HR Manager")]
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

        /// <summary>
        /// Retrieve all recruiter registration request
        /// </summary>
        /// <remarks>
        /// Only user with the role "HR Manager" is authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     GET /api/RecruiterRegistrationRequest/recruiter-regist-request
        /// 
        /// </remarks>
        /// <returns>A list of recruiter registration request data</returns>
        [Authorize(Roles = "HR Manager")]
        [HttpGet("recruiter-regist-request")]
        public async Task<IActionResult> GetAllRecruiterRegistrationRequest()
        {
            var cr = await _recruiterRegistrationRequestService.GetAllUnreviewedRecruiterRegistrationRequest();

            return Ok(cr);
        }
    }
}
