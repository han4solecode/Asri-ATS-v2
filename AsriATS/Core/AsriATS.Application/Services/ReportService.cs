using AsriATS.Application.Contracts;
using AsriATS.Application.Persistance;
using PdfSharpCore.Pdf;
using PdfSharpCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using TheArtOfDev.HtmlRenderer.Core;
using TheArtOfDev.HtmlRenderer.PdfSharp;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using AsriATS.Domain.Entities;
using Org.BouncyCastle.Asn1.Ocsp;
using AsriATS.Application.DTOs.Report;

namespace AsriATS.Application.Services
{
    public class ReportService:IReportService
    {
        private readonly IApplicationJobRepository _applicationJobRepository;
        private readonly IJobPostRepository _jobPostRepository;
        private readonly IUserService _userService;
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<AppUser> _userManager;
        private readonly IWorkflowSequenceRepository _workflowSequenceRepository;
        private readonly IWorkflowActionRepository _workflowActionRepository;
        private readonly IJobPostRequestRepository _jobPostRequestRepository;

        public ReportService(IApplicationJobRepository applicationJobRepository, IJobPostRepository jobPostRepository, IUserService userService, IHttpContextAccessor httpContextAccessor, UserManager<AppUser> userManager, IWorkflowSequenceRepository workflowSequenceRepository, IWorkflowActionRepository workflowActionRepository, IJobPostRequestRepository jobPostRequestRepository)
        {
            _applicationJobRepository = applicationJobRepository;
            _jobPostRepository = jobPostRepository;
            _userService = userService;
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _workflowSequenceRepository = workflowSequenceRepository;
            _workflowActionRepository = workflowActionRepository;
            _jobPostRequestRepository = jobPostRequestRepository;
        }

        public async Task<byte[]> GenerateOverallRecruitmentMetricsAsync()
        {
            // Get data from repository methods without date filtering
            var jobPosts = await _jobPostRepository.GetJobPostSummaryAsync(); 
            var applicationSummary = await _applicationJobRepository.GetApplicationSummaryAsync();
            var jobTotal = await _jobPostRepository.TotalJobPost();
            var totalApply = await _applicationJobRepository.TotalApplicationJob();

            // Start building the HTML content for the PDF
            string htmlContent = "<h1>Overall Recruitment Metrics</h1>";

            // Total Jobs Posted section
            htmlContent += $"<h2>Total Jobs Posted : {jobTotal}</h2>";
            htmlContent += "<table><thead><tr><td>Job Post ID</td><td>Created Date</td><td>Status</td><td>Company ID</td></tr></thead><tbody>";
            jobPosts.ForEach(job =>
            {
                htmlContent += $"<tr><td>{job.JobPostId}</td><td>{job.CreatedDate.ToShortDateString()}</td><td>{job.Status}</td><td>{job.CompanyId}</td></tr>";
            });
            htmlContent += "</tbody></table>";

            // Applications Summary section
            htmlContent += $"<h2>Applications Summary : {totalApply}</h2>";
            htmlContent += "<table><thead><tr><td>Application Job ID</td><td>Job Post ID</td><td>Uploaded Date</td><td>Status</td><td>Interview Date</td><td>Interview Scheduled</td></tr></thead><tbody>";
            applicationSummary.ForEach(app =>
            {
                htmlContent += $"<tr><td>{app.ApplicationJobId}</td><td>{app.JobPostId}</td><td>{app.UploadedDate.ToShortDateString()}</td><td>{app.Status}</td><td>{app.InterviewDate}</td><td>{app.InterviewScheduled}</td></tr>"; // Change to RequestDate if needed
            });
            htmlContent += "</tbody></table>";

            // Generate PDF using Polybioz
            return GeneratePdf(htmlContent);
        }

        public async Task<byte[]> GenerateDemographicReportAsync(string address)
        {
            var demographics = await _userService.GetDemographicOverviewAsync(address);

            // Start building the HTML content for the PDF
            string htmlContent = "<h1>Demographic Overview Report</h1>";
            htmlContent += $"<h3>Address Filter: {address}</h3>";
            htmlContent += "<table><thead><tr><td>User ID</td><td>Full Name</td><td>Tanggal Lahir</td><td>Address</td><td>Email</td><td>Phone Number</td></tr></thead><tbody>";

            foreach (var demographic in demographics)
            {
                htmlContent += $"<tr><td>{demographic.UserId}</td><td>{demographic.FullName}</td><td>{demographic.Dob}</td><td>{demographic.Address}</td><td>{demographic.Email}</td><td>{demographic.PhoneNumber}</td></tr>";
            }

            htmlContent += "</tbody></table>";

            // Generate PDF using Polybioz
            return GeneratePdf(htmlContent);
        }

        private byte[] GeneratePdf(string htmlContent)
        {
            var document = new PdfDocument();
            var config = new PdfGenerateConfig
            {
                PageOrientation = PageOrientation.Portrait,
                PageSize = PageSize.A4
            };

            string cssStr = File.ReadAllText(@"./Templates/ReportTemplates/style.css");
            CssData css = PdfGenerator.ParseStyleSheet(cssStr);

            PdfGenerator.AddPdfPages(document, htmlContent, config, css);

            using (var stream = new MemoryStream())
            {
                document.Save(stream, false);
                return stream.ToArray();
            }
        }

        public async Task<byte[]> GenerateRecruitmentFunnelReportAsync()
        {
            var userName = _httpContextAccessor.HttpContext!.User.Identity!.Name;
            var user = await _userManager.FindByNameAsync(userName!);

            //GetAll Recruitment Steps
            var allStages = await _workflowSequenceRepository.GetAllAsync(ws => ws.Workflow.WorkflowName == "Application Job Request");
            //GetAll Steps that Applicants did
            var recruitmentFunnel = await _workflowActionRepository.GetRecruitmentFlunnel(user!.CompanyId);

            var results = allStages
                .GroupJoin(recruitmentFunnel,
                    stage => stage.StepName,
                    funnel => funnel.StageName,
                    (stage, funnel) => new
                    {
                        stage.StepName,
                        Count = funnel.Select(f => f.Count).FirstOrDefault()
                    })
                .Select(r => new
                {
                    CompanyName = recruitmentFunnel.FirstOrDefault()?.CompanyName ?? "Unknown",
                    StageName = r.StepName,
                    Count = r.Count
                })
                .ToList();

            // Start building the HTML content for the PDF
            string htmlContent = "<h1>Recruitment Funnel Overview Report</h1>";
            htmlContent += $"<h3>Company: {results[0].CompanyName}</h3>";
            htmlContent += "<table><thead><tr><td>Recruitment Step</td><td>Count</td></thead><tbody>";

            foreach (var result  in results)
            {
                htmlContent += $"<tr><td>{result.StageName}</td><td>{result.Count}</td></tr>";
            }
            htmlContent += "</tbody></table>";
            // Generate PDF using Polybioz
            return GeneratePdf(htmlContent);
        }

        public async Task<byte[]> GenerateComplianceApprovalMetricsPdfAsync()
        {
            var metricsByCompany = await _jobPostRequestRepository.GetJobPostApprovalMetricsByCompanyAsync();

            // Start building the HTML content for the PDF
            string htmlContent = "<h1>Compliance and Approval Metrics Report</h1>";

            // Loop through each company's metrics and add to the HTML content
            foreach (var metrics in metricsByCompany)
            {
                htmlContent += $"<h2>Company ID: {metrics.CompanyId}</h2>";
                htmlContent += "<p>Total Approved: " + metrics.TotalApproved + "</p>";
                htmlContent += "<p>Total Rejected: " + metrics.TotalRejected + "</p>";
                htmlContent += "<p>Average Approval Time (in days): " + metrics.AverageApprovalTime.ToString("F2") + "</p>";
                htmlContent += "<hr>"; // Add a separator between companies
            }

            // Generate PDF using Polybioz
            return GeneratePdf(htmlContent);
        }

        public async Task<ApplicationJobSummaryDto> GetTotalAndApplicationSummaryAsync()
        {
            // Get data from repository methods without date filtering
            var jobPosts = await _jobPostRepository.GetJobPostSummaryAsync();
            var applicationSummary = await _applicationJobRepository.GetApplicationSummaryAsync();
            var jobTotal = await _jobPostRepository.TotalJobPost();
            var totalApply = await _applicationJobRepository.TotalApplicationJob();

            return new ApplicationJobSummaryDto
            {
                TotalApplications = totalApply,
                ApplicationSummaries = applicationSummary,
                TotalJobPosted = jobTotal,
                JobStatusDtos = jobPosts
            };
        }

        public async Task<IEnumerable<DemographicOverviewDto>> GetDemographicSummaryAsync(string address)
        {
            var demographics = await _userService.GetDemographicOverviewAsync(address);

            return demographics;
        }

        public async Task<List<ComplianceApprovalMetricsDto>> GetJobPostApprovalMetricsByCompanyAsync()
        {
            var job = await _jobPostRequestRepository.GetJobPostApprovalMetricsByCompanyAsync();
            return job;
        }
    }
}
