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

        [HttpGet("generate-overall-report")]
        public async Task<IActionResult> GenerateOverallReport()
        {
            var pdfBytes = await _reportService.GenerateOverallRecruitmentMetricsAsync();
            return File(pdfBytes, "application/pdf", "OverallReport.pdf");
        }

        [HttpGet("generate-demographic-report")]
        public async Task<IActionResult> GenerateDemographicReport(string address)
        {
            var pdfBytes = await _reportService.GenerateDemographicReportAsync(address);
            return File(pdfBytes, "application/pdf", "DemographicReport.pdf");
        }

        [Authorize(Roles = "Recruiter, HR Manager")]
        [HttpGet("generate-recruitment-funnel")]
        public async Task<IActionResult> GenerateRecruitmentFlunnelReport()
        {
            var pdfBytes = await _reportService.GenerateRecruitmentFunnelReportAsync();
            return File(pdfBytes, "application/pdf", "RecruitmentFunnelReport.pdf");
        }

        [HttpGet("Approval-time")] 
        public async Task<IActionResult> GenerateApprovalTime()
        {
            var pdfBytes = await _reportService.GenerateComplianceApprovalMetricsPdfAsync();
            return File(pdfBytes, "application/pdf", "ComplianceApprovalMetricsReport.pdf");
        }

        [HttpGet("ApprovalMetrics")]
        public async Task<IActionResult> ApprovalMetricsAsync()
        {
            var res = await _reportService.GetJobPostApprovalMetricsByCompanyAsync();
            return Ok(res);
        }

        [HttpGet("OverallMetrics")]
        public async Task<IActionResult> OverallRecruitmentMetricsAsync()
        {
            var res = await _reportService.GetTotalAndApplicationSummaryAsync();
            return Ok(res);
        }

        [HttpGet("DemographicsMetrics")]
        public async Task<IActionResult> OverallDemographicMetricsAsync(string address)
        {
            var res = await _reportService.GetDemographicSummaryAsync(address);
            return Ok(res);
        }
    }
}
