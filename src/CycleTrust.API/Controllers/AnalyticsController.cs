using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs;
using CycleTrust.Application.Services;

namespace CycleTrust.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ADMIN")]
    public class AnalyticsController : ControllerBase
    {
        private readonly IAnalyticsService _analyticsService;

        public AnalyticsController(IAnalyticsService analyticsService)
        {
            _analyticsService = analyticsService;
        }

        /// <summary>
        /// Get dashboard analytics data
        /// </summary>
        /// <param name="period">Period type: daily, weekly, or monthly</param>
        /// <param name="fromDate">Start date (optional)</param>
        /// <param name="toDate">End date (optional)</param>
        /// <returns>Complete dashboard data</returns>
        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardDto>> GetDashboard(
            [FromQuery] string period = "monthly",
            [FromQuery] DateTime? fromDate = null,
            [FromQuery] DateTime? toDate = null)
        {
            try
            {
                var data = await _analyticsService.GetDashboardDataAsync(period, fromDate, toDate);
                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi khi tải dữ liệu dashboard", error = ex.Message });
            }
        }
    }
}
