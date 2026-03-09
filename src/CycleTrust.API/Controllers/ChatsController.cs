using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs.Common;
using CycleTrust.Application.DTOs.Chat;
using CycleTrust.Application.Services;

namespace CycleTrust.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ChatsController : ControllerBase
{
    private readonly IChatService _chatService;

    public ChatsController(IChatService chatService)
    {
        _chatService = chatService;
    }

    private long GetUserId() => long.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpGet("conversations")]
    public async Task<ActionResult<ApiResponse<List<ChatConversationDto>>>> GetConversations()
    {
        try
        {
            var userId = GetUserId();
            var result = await _chatService.GetUserConversationsAsync(userId);
            return Ok(ApiResponse<List<ChatConversationDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<ChatConversationDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("conversations")]
    public async Task<ActionResult<ApiResponse<ChatConversationDto>>> GetOrCreateConversation(
        [FromBody] GetOrCreateConversationRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _chatService.GetOrCreateConversationAsync(userId, request.ListingId, request.SellerId);
            return Ok(ApiResponse<ChatConversationDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ChatConversationDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("conversations/{conversationId}/messages")]
    public async Task<ActionResult<ApiResponse<List<ChatMessageDto>>>> GetMessages(
        long conversationId,
        [FromQuery] int limit = 50)
    {
        try
        {
            var userId = GetUserId();
            var result = await _chatService.GetConversationMessagesAsync(conversationId, userId, limit);
            return Ok(ApiResponse<List<ChatMessageDto>>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<ChatMessageDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpPost("messages")]
    public async Task<ActionResult<ApiResponse<ChatMessageDto>>> SendMessage([FromBody] SendMessageRequest request)
    {
        try
        {
            var userId = GetUserId();
            var result = await _chatService.SendMessageAsync(userId, request);
            return Ok(ApiResponse<ChatMessageDto>.SuccessResponse(result));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<ChatMessageDto>.ErrorResponse(ex.Message));
        }
    }

    [HttpPut("conversations/{conversationId}/read")]
    public async Task<ActionResult<ApiResponse<object>>> MarkMessagesAsRead(long conversationId)
    {
        try
        {
            var userId = GetUserId();
            await _chatService.MarkMessagesAsReadAsync(conversationId, userId);
            return Ok(ApiResponse<object>.SuccessResponse(new { message = "Đã đánh dấu đọc tin nhắn" }));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse(ex.Message));
        }
    }
}
