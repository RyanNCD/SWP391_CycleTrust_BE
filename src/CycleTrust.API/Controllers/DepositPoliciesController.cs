using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs.Common;
using CycleTrust.Application.DTOs.DepositPolicy;
using CycleTrust.Application.Services;

namespace CycleTrust.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DepositPoliciesController : ControllerBase
{
    private readonly IDepositPolicyService _depositPolicyService;

    public DepositPoliciesController(IDepositPolicyService depositPolicyService)
    {
        _depositPolicyService = depositPolicyService;
    }

    /// <summary>
    /// Get active deposit policy (Public)
    /// </summary>
    [HttpGet("active")]
    public async Task<ActionResult<ApiResponse<DepositPolicyDto?>>> GetActivePolicy()
    {
        try
        {
            var result = await _depositPolicyService.GetActivePolicyAsync();
            return Ok(ApiResponse<DepositPolicyDto?>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DepositPolicyDto?>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Get all deposit policies (ADMIN)
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<DepositPolicyDto>>>> GetAllPolicies()
    {
        try
        {
            var result = await _depositPolicyService.GetAllPoliciesAsync();
            return Ok(ApiResponse<List<DepositPolicyDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<DepositPolicyDto>>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Create new deposit policy (ADMIN)
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPost]
    public async Task<ActionResult<ApiResponse<DepositPolicyDto>>> CreatePolicy([FromBody] CreateDepositPolicyRequest request)
    {
        try
        {
            var result = await _depositPolicyService.CreatePolicyAsync(request);
            return Ok(ApiResponse<DepositPolicyDto>.SuccessResponse(result, "Tạo deposit policy thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DepositPolicyDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Update deposit policy (ADMIN)
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPut("{id}")]
    public async Task<ActionResult<ApiResponse<DepositPolicyDto>>> UpdatePolicy(long id, [FromBody] UpdateDepositPolicyRequest request)
    {
        try
        {
            var result = await _depositPolicyService.UpdatePolicyAsync(id, request);
            return Ok(ApiResponse<DepositPolicyDto>.SuccessResponse(result, "Cập nhật policy thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DepositPolicyDto>.ErrorResponse(ex.Message));
        }
    }

    /// <summary>
    /// Set policy active/inactive (ADMIN)
    /// </summary>
    [Authorize(Roles = "ADMIN")]
    [HttpPatch("{id}/active")]
    public async Task<ActionResult<ApiResponse<DepositPolicyDto>>> SetActive(long id, [FromQuery] bool isActive)
    {
        try
        {
            var result = await _depositPolicyService.SetActiveAsync(id, isActive);
            return Ok(ApiResponse<DepositPolicyDto>.SuccessResponse(result, $"Policy đã được {(isActive ? "kích hoạt" : "vô hiệu hóa")}"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<DepositPolicyDto>.ErrorResponse(ex.Message));
        }
    }
}
