using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs.Common;
using CycleTrust.Application.DTOs.User;
using CycleTrust.Application.Services;

namespace CycleTrust.API.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = "ADMIN")]
public class AdminController : ControllerBase
{
    private readonly IUserManagementService _userManagementService;

    public AdminController(IUserManagementService userManagementService)
    {
        _userManagementService = userManagementService;
    }

    [HttpGet("users")]
    public async Task<ActionResult<ApiResponse<UserListResponse>>> GetAllUsers(
        [FromQuery] int page = 1,
        [FromQuery] int limit = 20,
        [FromQuery] string? role = null,
        [FromQuery] bool? isActive = null,
        [FromQuery] string? search = null)
    {
        try
        {
            var filter = new UserFilterRequest
            {
                Page = page,
                Limit = limit,
                Role = role,
                IsActive = isActive,
                Search = search
            };

            var result = await _userManagementService.GetAllUsersAsync(filter);
            return Ok(ApiResponse<UserListResponse>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserListResponse>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("users/{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> GetUserById(long id)
    {
        try
        {
            var user = await _userManagementService.GetUserByIdAsync(id);
            return Ok(ApiResponse<UserDto>.SuccessResponse(user));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("users")]
    public async Task<ActionResult<ApiResponse<UserDto>>> CreateUser([FromBody] CreateUserRequest request)
    {
        try
        {
            var user = await _userManagementService.CreateUserAsync(request);
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Tạo user thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("users/{id}")]
    public async Task<ActionResult<ApiResponse<UserDto>>> UpdateUser(long id, [FromBody] UpdateUserRequest request)
    {
        try
        {
            var user = await _userManagementService.UpdateUserAsync(id, request);
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Cập nhật user thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpDelete("users/{id}")]
    public async Task<ActionResult<ApiResponse<object?>>> DeleteUser(long id)
    {
        try
        {
            await _userManagementService.DeleteUserAsync(id);
            return Ok(ApiResponse<object?>.SuccessResponse(null, "Xóa user thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object?>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("users/pending-sellers")]
    public async Task<ActionResult<ApiResponse<List<UserDto>>>> GetPendingSellers()
    {
        try
        {
            var sellers = await _userManagementService.GetPendingSellersAsync();
            return Ok(ApiResponse<List<UserDto>>.SuccessResponse(sellers));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<UserDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("users/{id}/approve")]
    public async Task<ActionResult<ApiResponse<UserDto>>> ApproveSeller(long id)
    {
        try
        {
            var user = await _userManagementService.ApproveSellerAsync(id);
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Đã phê duyệt seller"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("users/{id}/reject")]
    public async Task<ActionResult<ApiResponse<UserDto>>> RejectSeller(long id)
    {
        try
        {
            var user = await _userManagementService.RejectSellerAsync(id);
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Đã từ chối seller"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPatch("users/{id}/toggle-active")]
    public async Task<ActionResult<ApiResponse<UserDto>>> ToggleUserActive(long id)
    {
        try
        {
            var user = await _userManagementService.ToggleActiveAsync(id);
            return Ok(ApiResponse<UserDto>.SuccessResponse(user, "Đã cập nhật trạng thái user"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<UserDto>.ErrorResponse(ex.Message));
        }
    }
}
