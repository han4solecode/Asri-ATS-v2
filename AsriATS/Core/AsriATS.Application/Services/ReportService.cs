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

namespace AsriATS.Application.Services
{
    public class ReportService:IReportService
    {
        private readonly IApplicationJobRepository _applicationJobRepository;
        private readonly IJobPostRepository _jobPostRepository;
        private readonly IUserService _userService;

        public ReportService(IApplicationJobRepository applicationJobRepository, IJobPostRepository jobPostRepository, IUserService userService)
        {
            _applicationJobRepository = applicationJobRepository;
            _jobPostRepository = jobPostRepository;
            _userService = userService;
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
            htmlContent += "<h2>Total Jobs Posted</h2>";
            htmlContent += "<table><thead><tr><td>Job Post ID</td><td>Created Date</td><td>Status</td></tr></thead><tbody>";
            jobPosts.ForEach(job =>
            {
                htmlContent += $"<tr><td>{job.JobPostId}</td><td>{job.CreatedDate.ToShortDateString()}</td><td>{job.Status}</td></tr>";
            });
            htmlContent += "</tbody></table>";

            // Applications Summary section
            htmlContent += "<h2>Applications Summary</h2>";
            htmlContent += "<table><thead><tr><td>Application Job ID</td><td>Uploaded Date</td><td>Status</td></tr></thead><tbody>";
            applicationSummary.ForEach(app =>
            {
                htmlContent += $"<tr><td>{app.ApplicationJobId}</td><td>{app.UploadedDate.ToShortDateString()}</td><td>{app.Status}</td></tr>"; // Change to RequestDate if needed
            });
            htmlContent += "</tbody></table>";

            // Applications Total section
            htmlContent += "<h2>Applications Total</h2>";
            htmlContent += "<table><thead><tr><td>Total Applications</td></tr></thead><tbody>";
            htmlContent += $"<tr><td>{totalApply}</td></tr>"; // Just display the total count
            htmlContent += "</tbody></table>";

            // Total Jobs Posted Count section
            htmlContent += "<h2>Total Job Posts</h2>";
            htmlContent += "<table><thead><tr><td>Total Job Posts</td></tr></thead><tbody>";
            htmlContent += $"<tr><td>{jobTotal}</td></tr>"; // Just display the total count
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
            htmlContent += "<table><thead><tr><td>User ID</td><td>UserName</td><td>Full Name</td><td>Address</td><td>Email</td><td>Phone Number</td></tr></thead><tbody>";

            foreach (var demographic in demographics)
            {
                htmlContent += $"<tr><td>{demographic.UserId}</td><td>{demographic.Username}</td><td>{demographic.FullName}</td><td>{demographic.Address}</td><td>{demographic.Email}</td><td>{demographic.PhoneNumber}</td></tr>";
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
    }
}
