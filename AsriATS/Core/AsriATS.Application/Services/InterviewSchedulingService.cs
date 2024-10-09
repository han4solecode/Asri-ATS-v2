using AsriATS.Application.Contracts;
using AsriATS.Application.DTOs;
using AsriATS.Application.DTOs.Email;
using AsriATS.Application.DTOs.InterivewScheduling;
using AsriATS.Application.DTOs.Request;
using AsriATS.Application.Persistance;
using AsriATS.Domain.Entities;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Org.BouncyCastle.Asn1.Ocsp;
using System.Diagnostics;

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

            if (process.WorkflowSequence.StepName != "HR Manager Set Interview Schedule")
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

            // next step rule check
            var nextStepRule = await _nextStepRuleRepository.GetFirstOrDefaultAsync(nsr => nsr.CurrentStepId == process.CurrentStepId && nsr.ConditionValue == reviewRequest.Action);

            if (nextStepRule == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = $"Action is not valid"
                };
            };

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
            var nextStepId = nextStepRule!.NextStepId;

            // update process
            process.Status = $"{reviewRequest.Action} by {userRole}";
            process.CurrentStepId = nextStepId;
            await _processRepository.UpdateAsync(process);

            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Interview Process Reviewed Sucessfuly"
            };
        }

        public async Task<BaseResponseDto> UpdateInterviewSchedule(UpdateInterviewScheduleDto updateInterview)
        {
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);
            var userRoles = await _userManager.GetRolesAsync(user!);
            var userRole = userRoles.Single();

            var process = await _processRepository.GetByIdAsync(updateInterview.ProcessId);

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

            // check nextStepRule
            var nextStepRule = await _nextStepRuleRepository.GetFirstOrDefaultAsync(nsr => nsr.CurrentStepId == process.CurrentStepId && nsr.ConditionValue == "Update");

            if (nextStepRule == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = $"Action is not valid"
                };
            };

            if (process.InterviewScheduleNavigation!.IsConfirmed != null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Interview schedule can not be updated"
                };
            }

            var jobApplicationCompanyId = process.ApplicationJobNavigation.Select(x => x.JobPostNavigation.CompanyId).Single();

            if (jobApplicationCompanyId != user!.CompanyId || process.WorkflowSequence.Role.Name != userRole)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Unauthorize to review request"
                };
            }

            var interview = await _interviewSchedulingRepository.GetByIdAsync(process.InterviewScheduleNavigation.InterviewSchedulingId);

            if (interview == null)
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = "Interview schedule does not exist"
                };
            }

            // update interview schedule
            interview.InterviewTime = updateInterview.InterviewTime;

            await _interviewSchedulingRepository.UpdateAsync(interview);

            // create new workflow action
            var newWorkflowAction = new WorkflowAction
            {
                ProcessId = process.ProcessId,
                StepId = process.CurrentStepId,
                ActorId = user.Id,
                Action = "Update",
                ActionDate = DateTime.UtcNow,
                Comments = updateInterview.Comment,
            };

            await _workflowActionRepository.CreateAsync(newWorkflowAction);

            // get nextStepId
            var nextStepId = nextStepRule.NextStepId;

            // update process
            process.Status = $"{newWorkflowAction.Action} by {userRole}";
            process.CurrentStepId = nextStepId;
            await _processRepository.UpdateAsync(process);

            // TODO: send email notification


            return new BaseResponseDto
            {
                Status = "Success",
                Message = "Interview schedule updated successfuly"
            };
        }

        public async Task<BaseResponseDto> InterviewConfirmation(ReviewRequestDto reviewRequest)
        {
            var review = await ReviewInterviewProcess(reviewRequest);

            if (review.Status == "Error")
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = review.Message,
                };
            }
            else
            {
                if (reviewRequest.Action == "Confirm")
                {
                    var process = await _processRepository.GetByIdAsync(reviewRequest.ProcessId);

                    if (process == null)
                    {
                        return new BaseResponseDto
                        {
                            Status = "Error",
                            Message = "Process does not exist"
                        };
                    }

                    process.InterviewScheduleNavigation!.IsConfirmed = true;
                    await _processRepository.UpdateAsync(process);

                    // TODO: send schedule confirmation email


                    return new BaseResponseDto
                    {
                        Status = "Success",
                        Message = $"Interview {reviewRequest.Action} successfuly"
                    };
                }
                // TODO: send reschedule request email


                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = $"Interview {reviewRequest.Action} successfuly"
                };
            }
        }

        public async Task<BaseResponseDto> MarkInterviewAsComplete(MarkInterviewAsCompleteDto markInterviewAsComplete)
        {
            var reviewRequest = new ReviewRequestDto
            {
                ProcessId = markInterviewAsComplete.ProcessId,
                Action = "Complete",
                Comment = markInterviewAsComplete.Comment
            };

            var review = await ReviewInterviewProcess(reviewRequest);

            if (review.Status == "Error")
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = review.Message,
                };
            }
            else
            {
                var process = await _processRepository.GetByIdAsync(markInterviewAsComplete.ProcessId);

                if (process == null)
                {
                    return new BaseResponseDto
                    {
                        Status = "Error",
                        Message = "Process does not exist"
                    };
                }

                process.InterviewScheduleNavigation!.InterviewersComments = markInterviewAsComplete.InterviewersComments;
                await _processRepository.UpdateAsync(process);

                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = $"Interview marked as complete successfuly"
                };
            }
        }

        public async Task<BaseResponseDto> ReviewInterviewResult(ReviewRequestDto reviewRequest)
        {
            var review = await ReviewInterviewProcess(reviewRequest);

            if (review.Status == "Error")
            {
                return new BaseResponseDto
                {
                    Status = "Error",
                    Message = review.Message,
                };
            }
            else
            {
                var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
                var user = await _userManager.FindByNameAsync(userName!);

                var process = await _processRepository.GetByIdAsync(reviewRequest.ProcessId);

                var requesterEmail = process!.Requester.Email;

                if (reviewRequest.Action == "Offer")
                {
                    // send offering email
                    var emailTemplate = File.ReadAllText(@"./Templates/EmailTemplates/JobOfferLetter.html");

                    var a = process.ApplicationJobNavigation.Select(x => x.JobPostNavigation.MinSalary).SingleOrDefault();

                    emailTemplate = emailTemplate.Replace("{{Name}}", $"{process.Requester.FirstName} {process.Requester.LastName}");
                    emailTemplate = emailTemplate.Replace("{{JobTitle}}", process.ApplicationJobNavigation.Select(x => x.JobPostNavigation.JobTitle).SingleOrDefault());
                    emailTemplate = emailTemplate.Replace("{{CompanyName}}", process.ApplicationJobNavigation.Select(x => x.JobPostNavigation.CompanyIdNavigation.Name).SingleOrDefault());
                    emailTemplate = emailTemplate.Replace("{{Salary}}", $"{process.ApplicationJobNavigation.Select(x => x.JobPostNavigation.MinSalary).SingleOrDefault()}");
                    emailTemplate = emailTemplate.Replace("{{CompanyAddress}}", process.ApplicationJobNavigation.Select(x => x.JobPostNavigation.CompanyIdNavigation.Address).SingleOrDefault());
                    emailTemplate = emailTemplate.Replace("{{HREmail}}", user!.Email);
                    emailTemplate = emailTemplate.Replace("{{HRName}}", $"{user.FirstName} {user.LastName}");

                    var offerEmail = new EmailDataDto
                    {
                        EmailCCIds = [requesterEmail],
                        EmailSubject = "Hiring Process Update",
                        EmailBody = emailTemplate
                    };

                    await _emailService.SendEmailAsync(offerEmail);

                    return new BaseResponseDto
                    {
                        Status = "Success",
                        Message = "Interview result reviewed successfuly"
                    };
                }

                // send rejection email
                var emailTemplate1 = File.ReadAllText(@"./Templates/EmailTemplates/JobRejectionLetter.html");

                emailTemplate1 = emailTemplate1.Replace("{{Name}}", $"{process.Requester.FirstName} {process.Requester.LastName}");
                emailTemplate1 = emailTemplate1.Replace("{{JobTitle}}", process.ApplicationJobNavigation.Select(x => x.JobPostNavigation.JobTitle).SingleOrDefault());
                emailTemplate1 = emailTemplate1.Replace("{{CompanyName}}", process.ApplicationJobNavigation.Select(x => x.JobPostNavigation.CompanyIdNavigation.Name).SingleOrDefault());
                emailTemplate1 = emailTemplate1.Replace("{{HRName}}", $"{user!.FirstName} {user.LastName}");

                var rejectionEmail = new EmailDataDto
                {
                    EmailCCIds = [requesterEmail],
                    EmailSubject = "Hiring Process Update",
                    EmailBody = emailTemplate1
                };

                await _emailService.SendEmailAsync(rejectionEmail);

                return new BaseResponseDto
                {
                    Status = "Success",
                    Message = "Interview result reviewed successfuly"
                };
            }
        }

        public async Task<IEnumerable<object>> GetAllUnconfirmedInterviewSchedules()
        {
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);
            var userRoles = await _userManager.GetRolesAsync(user!);
            var userRole = userRoles.Single();

            IEnumerable<InterviewScheduling> interviewingSchedules;

            if (userRole == "Applicant")
            {
                interviewingSchedules = await _interviewSchedulingRepository.GetAllInterviewSchedulesAsync(i => i.ApplicationIdNavigation.UserId == user!.Id && i.IsConfirmed != true);
            }
            else
            {
                interviewingSchedules = await _interviewSchedulingRepository.GetAllInterviewSchedulesAsync(i => i.ApplicationIdNavigation.JobPostNavigation.CompanyId == user!.CompanyId && i.IsConfirmed != true);
            }

            var result = interviewingSchedules.Select(i => new
            {
                ApplicationId = i.ApplicationId,
                ApplicantName = $"{i.ApplicationIdNavigation.UserIdNavigation.FirstName} {i.ApplicationIdNavigation.UserIdNavigation.LastName}",
                JobTitle = i.ApplicationIdNavigation.JobPostNavigation.JobTitle,
                InterviewTime = i.InterviewTime,
                Location = i.Location,
                Interviewers = i.Interviewer,
            }).ToList();

            return result;
        }

        public async Task<IEnumerable<object>> GetAllConfirmedInterviewSchedules()
        {
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);
            var userRoles = await _userManager.GetRolesAsync(user!);
            var userRole = userRoles.Single();

            IEnumerable<InterviewScheduling> interviewingSchedules;

            if (userRole == "Applicant")
            {
                interviewingSchedules = await _interviewSchedulingRepository.GetAllInterviewSchedulesAsync(i => i.ApplicationIdNavigation.UserId == user!.Id && i.IsConfirmed == true);
            }
            else
            {
                interviewingSchedules = await _interviewSchedulingRepository.GetAllInterviewSchedulesAsync(i => i.ApplicationIdNavigation.JobPostNavigation.CompanyId == user!.CompanyId && i.IsConfirmed == true);
            }

            var result = interviewingSchedules.Select(i => new
            {
                ApplicationId = i.ApplicationId,
                ApplicantName = $"{i.ApplicationIdNavigation.UserIdNavigation.FirstName} {i.ApplicationIdNavigation.UserIdNavigation.LastName}",
                JobTitle = i.ApplicationIdNavigation.JobPostNavigation.JobTitle,
                InterviewTime = i.InterviewTime,
                Location = i.Location,
                Interviewers = i.Interviewer,
            }).ToList();

            return result;
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
