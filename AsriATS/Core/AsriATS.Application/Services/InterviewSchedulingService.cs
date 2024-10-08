using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Email;
using AsriATS.Application.DTOs.InterivewScheduling;
using AsriATS.Application.DTOs.Request;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;

namespace AsriATS.Application.Services
{
    public class InterviewSchedulingService : IInterviewSchedulingService
    {
        private readonly INextStepRuleRepository _nextStepRuleRepository;
        private readonly IProcessRepository _processRepository;
        private readonly IWorkflowActionRepository _workflowActionRepository;
        private readonly UserManager<AppUser> _userManager;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IEmailService _emailService;
        private readonly IApplicationJobRepository _applicationJobRepository;
        private readonly IInterviewSchedulingRepository _interviewSchedulingRepository;

        public InterviewSchedulingService(INextStepRuleRepository nextStepRuleRepository, IProcessRepository processRepository, IWorkflowActionRepository workflowActionRepository, UserManager<AppUser> userManager, IHttpContextAccessor httpContextAccessor, IEmailService emailService, IApplicationJobRepository applicationJobRepository, IInterviewSchedulingRepository interviewSchedulingRepository)
        {
            _nextStepRuleRepository = nextStepRuleRepository;
            _processRepository = processRepository;
            _workflowActionRepository = workflowActionRepository;
            _userManager = userManager;
            _httpContextAccessor = httpContextAccessor;
            _emailService = emailService;
            _applicationJobRepository = applicationJobRepository;
            _interviewSchedulingRepository = interviewSchedulingRepository;
        }

        public async Task<BaseResponseDto> SetInterviewSchedule(InterviewSchedulingRequestDto request)
        {
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);
            var userRoles = await _userManager.GetRolesAsync(user!);
            var userRole = userRoles.Single();

            var applicantJob = await _applicationJobRepository.GetByIdAsync(request.ApplicationJobId);
            if (applicantJob == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Applicant Job Not Found"
                };
            }

            if (applicantJob.JobPostNavigation.CompanyId != user!.CompanyId)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "You do not have authorize to make interview schedule with the applicant job"
                };
            }

            // get process
            var process = await _processRepository.GetByIdAsync(applicantJob.ProcessId);

            if (process == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Process does not exist"
                };
            }

            if (process.WorkflowSequence.StepName != "Shortlisted")
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Process is not for this step"
                };
            }

            var newWorkflowAction = new WorkflowAction
            {
                ProcessId = process.ProcessId,
                StepId = process.CurrentStepId,
                ActorId = user.Id,
                Action = request.Action,
                ActionDate = DateTime.UtcNow,
                Comments = request.Comment
            };

            await _workflowActionRepository.CreateAsync(newWorkflowAction);

            // get nextStepId
            var nextStepRule = await _nextStepRuleRepository.GetFirstOrDefaultAsync(nsr => nsr.CurrentStepId == process.CurrentStepId && nsr.ConditionValue == request.Action);
            var nextStepId = nextStepRule!.NextStepId;

            // update process
            process.Status = $"{request.Action} by {userRole}";
            process.CurrentStepId = nextStepId;
            await _processRepository.UpdateAsync(process);


            // Parse string with offset
            DateTimeOffset interviewTimeWithOffset = DateTimeOffset.Parse(request.InterviewTime);
            DateTime interviewDate = interviewTimeWithOffset.DateTime;
            string formattedDate = interviewDate.ToString("d MMMM yyyy");
            string interviewTime = interviewTimeWithOffset.ToString("hh:mm tt");

            // Konversi ke UTC
            DateTime utcTime = interviewTimeWithOffset.UtcDateTime;


            var newInterviewSchedule = new InterviewScheduling
            {
                ApplicationId = request.ApplicationJobId,
                Interviewer = request.Interviewers,
                InterviewTime = utcTime,
                InterviewType = request.InterviewType,
                Location = request.Location,
                ProcessId = process.ProcessId,
            };

            await _interviewSchedulingRepository.CreateAsync(newInterviewSchedule);


            // send email notification
            var emailTemplate = File.ReadAllText(@"./Templates/EmailTemplates/SetInterviewSchedule.html");

            emailTemplate = emailTemplate.Replace("{{Name}}", $"{process.Requester.FirstName} {process.Requester.LastName}");
            emailTemplate = emailTemplate.Replace("{{Date}}", formattedDate);
            emailTemplate = emailTemplate.Replace("{{Time}}", interviewTime);
            emailTemplate = emailTemplate.Replace("{{RecruiterName}}", $"{user.FirstName} {user.LastName}");

            request.InterviewerEmails.Add(user.Email!);

            var mail = new EmailDataDto
            {
                EmailToIds = [applicantJob.Email],
                EmailCCIds = request.InterviewerEmails,
                EmailSubject = "Interview Schedule",
                EmailBody = emailTemplate
            };

            await _emailService.SendEmailAsync(mail);

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Set Interview Schedule Sucessfuly"
            };
        }

        public async Task<BaseResponseDto> ReviewInterviewProcess(ReviewRequestDto reviewRequest)
        {
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);
            var userRoles = await _userManager.GetRolesAsync(user!);
            var userRole = userRoles.Single();

            var process = await _processRepository.GetByIdAsync(reviewRequest.ProcessId);

            if (process == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Process does not exist"
                };
            }

            if (process.WorkflowSequence.RequiredRole == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Process already finished"
                };
            }

            var jobApplicationCompanyId = process.ApplicationJobNavigation.Select(x => x.JobPostNavigation.CompanyId).Single();

            if (user!.CompanyId != null) // check if user has a company
            {
                if (jobApplicationCompanyId != user!.CompanyId || process.WorkflowSequence.Role.Name != userRole)
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "Unauthorize to review request"
                    };
                }
            }
            else // applicant check
            {
                if (process.RequesterId != user.Id || process.WorkflowSequence.Role.Name != userRole)
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "Unauthorize to review request"
                    };
                }
            }

            var newWorkflowAction = new WorkflowAction
            {
                ProcessId = process.ProcessId,
                StepId = process.CurrentStepId,
                ActorId = user.Id,
                Action = reviewRequest.Action,
                ActionDate = DateTime.UtcNow,
                Comments = reviewRequest.Comment
            };

            await _workflowActionRepository.CreateAsync(newWorkflowAction);

            // get nextStepId
            var nextStepRule = await _nextStepRuleRepository.GetFirstOrDefaultAsync(nsr => nsr.CurrentStepId == process.CurrentStepId && nsr.ConditionValue == reviewRequest.Action);
            var nextStepId = nextStepRule!.NextStepId;

            // update process
            process.Status = $"{reviewRequest.Action} by {userRole}";
            process.CurrentStepId = nextStepId;
            await _processRepository.UpdateAsync(process);

            // email notification


            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Job Application Reviewed Sucessfuly"
            };
        }

        public async Task<BaseResponseDto> UpdateInterviewSchedule()
        {
            // processId
            // DateTime

            // create new workflow action

            // update InterviewScheduling

            // update process

            // return
            throw new NotImplementedException();
        }

        // public async Task<BaseResponseDto> SetCompleteInterview
        // create new workflow action
        // set interviewerComments
        // ReviewInterviewProcess(int ProcessId, string Action, string Comments)

        // getInterviewSchedule (applicant, recruiter, and hr manager)

        // 1. applicant harus bisa liat jadwal interview yang harus di confirm / reschedule
        // 2. HR Manager harus bisa liat request reschedule dari applicant
        // 3. HR Manager harus bisa liat hasil interview yang udah selesai
    }
}
