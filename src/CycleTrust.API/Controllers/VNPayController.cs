using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs;
using CycleTrust.Application.Services;

namespace CycleTrust.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class VNPayController : ControllerBase
{
    private readonly IVNPayService _vnPayService;
    private readonly ILogger<VNPayController> _logger;

    public VNPayController(IVNPayService vnPayService, ILogger<VNPayController> logger)
    {
        _vnPayService = vnPayService;
        _logger = logger;
    }

    /// <summary>
    /// Create VNPay payment URL for deposit payment
    /// </summary>
    [HttpPost("create-payment")]
    [Authorize]
    public async Task<ActionResult<VNPayPaymentResponseDto>> CreatePayment([FromBody] VNPayPaymentRequestDto request)
    {
        try
        {
            var ipAddress = HttpContext.Connection.RemoteIpAddress?.ToString() ?? "127.0.0.1";
            var result = await _vnPayService.CreatePaymentUrl(request, ipAddress);

            if (!result.Success)
            {
                return BadRequest(result);
            }

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating VNPay payment");
            return StatusCode(500, new VNPayPaymentResponseDto
            {
                Success = false,
                Message = "Lỗi tạo thanh toán VNPay"
            });
        }
    }

    /// <summary>
    /// Process VNPay callback/return URL
    /// </summary>
    [HttpGet("callback")]
    [AllowAnonymous]
    public async Task<ActionResult<VNPayReturnDto>> ProcessCallback()
    {
        try
        {
            var queryParams = Request.Query
                .ToDictionary(k => k.Key, v => v.Value.ToString());

            var result = await _vnPayService.ProcessCallback(queryParams);

            _logger.LogInformation("VNPay callback processed: {TransactionId}, Success: {Success}", 
                result.TransactionId, result.Success);

            return Ok(result);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay callback");
            return StatusCode(500, new VNPayReturnDto
            {
                Success = false,
                Message = "Lỗi xử lý callback VNPay"
            });
        }
    }

    /// <summary>
    /// Process VNPay IPN (Instant Payment Notification)
    /// </summary>
    [HttpGet("ipn")]
    [AllowAnonymous]
    public async Task<IActionResult> ProcessIPN()
    {
        try
        {
            var queryParams = Request.Query
                .ToDictionary(k => k.Key, v => v.Value.ToString());

            var result = await _vnPayService.ProcessCallback(queryParams);

            _logger.LogInformation("VNPay IPN processed: {TransactionId}, Success: {Success}", 
                result.TransactionId, result.Success);

            // VNPay expects specific response format
            if (result.Success)
            {
                return Ok(new { RspCode = "00", Message = "Confirm Success" });
            }
            else
            {
                return Ok(new { RspCode = "99", Message = "Confirm Fail" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing VNPay IPN");
            return Ok(new { RspCode = "99", Message = "Exception" });
        }
    }
}
