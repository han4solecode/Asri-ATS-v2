using AsriATS.Application.Contracts;
using AsriATS.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AsriATS.WebAPI.Controllers
{
    [ApiController]
    [Route("api/[Controller]")]
    public class ReportController : ControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(IReportService reportService)
        {
            _reportService = reportService;
        }

        /// <summary>
        /// Generates an overall recruitment metrics PDF report.
        /// </summary>
        /// <remarks>
        /// This API endpoint generates a PDF report of overall recruitment metrics, such as job posts created, applications received, and their statuses.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/reports/generate-overall-report
        /// 
        /// The response includes a downloadable PDF report.
        /// </remarks>
        /// <returns>Returns a PDF file with overall recruitment metrics.</returns>
        [HttpGet("generate-overall-report")]
        public async Task<IActionResult> GenerateOverallReport()
        {
            var pdfBytes = await _reportService.GenerateOverallRecruitmentMetricsAsync();
            return File(pdfBytes, "application/pdf", "OverallReport.pdf");
        }

        /// <summary>
        /// Generates a demographic report PDF based on the provided address.
        /// </summary>
        /// <remarks>
        /// This API endpoint generates a PDF report of demographic metrics based on the given address parameter.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/reports/generate-demographic-report?address={address}
        /// 
        /// The response includes a downloadable PDF report.
        /// </remarks>
        /// <param name="address">The address or location filter for the demographic report.</param>
        /// <returns>Returns a PDF file with demographic data.</returns>
        [HttpGet("generate-demographic-report")]
        public async Task<IActionResult> GenerateDemographicReport(string address)
        {
            var pdfBytes = await _reportService.GenerateDemographicReportAsync(address);
            return File(pdfBytes, "application/pdf", "DemographicReport.pdf");
        }

        /// <summary>
        /// Generates a recruitment funnel PDF report.
        /// </summary>
        /// <remarks>
        /// This API endpoint generates a PDF report that shows the recruitment funnel, 
        /// including steps like applications submitted, interviews, and final hiring decisions.
        /// Only "Recruiter" and "HR Manager" roles are authorized to access this endpoint.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/reports/generate-recruitment-funnel
        /// 
        /// The response includes a downloadable PDF report.
        /// </remarks>
        /// <returns>Returns a PDF file with recruitment funnel data.</returns>
        [Authorize(Roles = "Recruiter, HR Manager")]
        [HttpGet("generate-recruitment-funnel")]
        public async Task<IActionResult> GenerateRecruitmentFlunnelReport()
        {
            var pdfBytes = await _reportService.GenerateRecruitmentFunnelReportAsync();
            return File(pdfBytes, "application/pdf", "RecruitmentFunnelReport.pdf");
        }

        /// <summary>
        /// Generates a compliance and approval metrics PDF report.
        /// </summary>
        /// <remarks>
        /// This API endpoint generates a PDF report containing data about compliance checks and job post approval times.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/reports/Approval-time
        /// 
        /// The response includes a downloadable PDF report.
        /// </remarks>
        /// <returns>Returns a PDF file with compliance and approval metrics data.</returns>
        [HttpGet("Approval-time")] 
        public async Task<IActionResult> GenerateApprovalTime()
        {
            var pdfBytes = await _reportService.GenerateComplianceApprovalMetricsPdfAsync();
            return File(pdfBytes, "application/pdf", "ComplianceApprovalMetricsReport.pdf");
        }

        /// <summary>
        /// Retrieves job post approval metrics for all companies.
        /// </summary>
        /// <remarks>
        /// This API endpoint retrieves metrics related to job post approvals, such as the number of approved and rejected posts, and average approval times, 
        /// aggregated by company.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/reports/ApprovalMetrics
        /// 
        /// The response includes a JSON object with job post approval metrics.
        /// </remarks>
        /// <returns>Returns approval metrics data for job posts by company.</returns>
        [HttpGet("ApprovalMetrics")]
        public async Task<IActionResult> ApprovalMetricsAsync()
        {
            var res = await _reportService.GetJobPostApprovalMetricsByCompanyAsync();
            return Ok(res);
        }

        /// <summary>
        /// Retrieves overall recruitment metrics, including total jobs posted and application summaries.
        /// </summary>
        /// <remarks>
        /// This API endpoint retrieves overall recruitment metrics, such as the total number of jobs posted, 
        /// the number of applications received, and their status distribution.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/reports/OverallMetrics
        /// 
        /// The response includes a JSON object with overall recruitment metrics.
        /// </remarks>
        /// <returns>Returns overall recruitment metrics.</returns>
        [HttpGet("OverallMetrics")]
        public async Task<IActionResult> OverallRecruitmentMetricsAsync()
        {
            var res = await _reportService.GetTotalAndApplicationSummaryAsync();
            return Ok(res);
        }

        /// <summary>
        /// Retrieves demographic metrics based on the provided address filter.
        /// </summary>
        /// <remarks>
        /// This API endpoint retrieves demographic metrics for recruitment, such as the number of applications from different 
        /// regions or locations based on the given address.
        /// 
        /// Sample request:
        /// 
        ///     GET https://localhost:7080/api/reports/DemographicsMetrics?address={address}
        /// 
        /// The response includes a JSON object with demographic data.
        /// </remarks>
        /// <param name="address">The address or location filter for the demographic report.</param>
        /// <returns>Returns demographic data based on the provided address filter.</returns>
        [HttpGet("DemographicsMetrics")]
        public async Task<IActionResult> OverallDemographicMetricsAsync(string address)
        {
            var res = await _reportService.GetDemographicSummaryAsync(address);
            return Ok(res);
        }
    }
}
