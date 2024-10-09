using AsriATS.Application.Contracts;
using AsriATS.Application.Services;
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

        [HttpGet("generate-recruitment-flunnel")]
        public async Task<IActionResult> GenerateRecruitmentFlunnelReport()
        {
            var response = await _reportService.GenerateRecruitmentFunnelReportAsync();
            return Ok(response);
        }
    }
}
