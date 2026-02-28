using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs.Auth;
using CycleTrust.Application.DTOs.Common;
using CycleTrust.Application.Services;

namespace CycleTrust.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    protected long GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.Parse(userIdClaim!);
    }

    protected string GetUserRole()
    {
        return User.FindFirstValue(ClaimTypes.Role)!;
    }

    [HttpPost("register")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _authService.RegisterAsync(request);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Đăng ký thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("login")]
    public async Task<ActionResult<ApiResponse<AuthResponse>>> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _authService.LoginAsync(request);
            return Ok(ApiResponse<AuthResponse>.SuccessResponse(result, "Đăng nhập thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<AuthResponse>.ErrorResponse(ex.Message));
        }
    }

    [Authorize]
    [HttpGet("me")]
    public ActionResult<ApiResponse<object>> GetCurrentUser()
    {
        var userId = GetUserId();
        var userName = User.FindFirstValue(ClaimTypes.Name);
        var role = GetUserRole();

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            userId,
            userName,
            role
        }));
    }
}
