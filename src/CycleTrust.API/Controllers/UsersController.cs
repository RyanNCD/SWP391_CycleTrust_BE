using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs.Common;
using CycleTrust.Application.DTOs.User;
using CycleTrust.Application.Services;

namespace CycleTrust.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    protected long GetUserId()
    {
        var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
        return long.Parse(userIdClaim!);
    }

    [HttpPut("profile")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _userService.UpdateProfileAsync(userId, request);
            return Ok(ApiResponse<UserDto>.SuccessResponse(result, "Cập nhật hồ sơ thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating profile for user {UserId}", GetUserId());
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("change-password")]
    public async Task<ActionResult<ApiResponse<object>>> ChangePassword([FromBody] ChangePasswordRequest request)
    {
        try
        {
            var userId = GetUserId();
            await _userService.ChangePasswordAsync(userId, request);
            return Ok(ApiResponse<object>.SuccessResponse(new { }, "Đổi mật khẩu thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error changing password for user {UserId}", GetUserId());
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("avatar")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UploadAvatar(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResponse("Vui lòng chọn file"));
            }

            var allowedTypes = new[] { "image/jpeg", "image/jpg", "image/png", "image/gif" };
            if (!allowedTypes.Contains(file.ContentType.ToLower()))
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResponse("Chỉ chấp nhận file ảnh (JPG, PNG, GIF)"));
            }

            if (file.Length > 5 * 1024 * 1024)
            {
                return BadRequest(ApiResponse<UserDto>.ErrorResponse("Kích thước file không được vượt quá 5MB"));
            }

            var userId = GetUserId();
            var result = await _userService.UploadAvatarAsync(userId, file);
            return Ok(ApiResponse<UserDto>.SuccessResponse(result, "Tải ảnh đại diện thành công"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading avatar for user {UserId}", GetUserId());
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
    }
}
