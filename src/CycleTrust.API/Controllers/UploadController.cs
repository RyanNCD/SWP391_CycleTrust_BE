using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs.Common;

namespace CycleTrust.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class UploadController : ControllerBase
{
    private readonly IWebHostEnvironment _environment;
    private readonly ILogger<UploadController> _logger;

    public UploadController(IWebHostEnvironment environment, ILogger<UploadController> logger)
    {
        _environment = environment;
        _logger = logger;
    }

    [Authorize(Roles = "INSPECTOR,ADMIN")]
    [HttpPost("inspection-report")]
    public async Task<ActionResult<ApiResponse<string>>> UploadInspectionReport(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<string>.ErrorResponse("No file uploaded"));

            // Validate file type
            var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".jpg", ".jpeg", ".png" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiResponse<string>.ErrorResponse("Invalid file type. Allowed: PDF, DOC, DOCX, JPG, PNG"));

            // Validate file size (max 10MB)
            if (file.Length > 10 * 1024 * 1024)
                return BadRequest(ApiResponse<string>.ErrorResponse("File size exceeds 10MB limit"));

            // Create uploads directory if not exists
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "inspections");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return URL
            var fileUrl = $"/uploads/inspections/{fileName}";
            
            _logger.LogInformation("File uploaded successfully: {FileName}", fileName);
            
            return Ok(ApiResponse<string>.SuccessResponse(fileUrl, "File uploaded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Error uploading file"));
        }
    }

    [Authorize(Roles = "INSPECTOR,ADMIN,SELLER")]
    [HttpPost("listing-image")]
    public async Task<ActionResult<ApiResponse<string>>> UploadListingImage(IFormFile file)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(ApiResponse<string>.ErrorResponse("No file uploaded"));

            // Validate file type
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".gif", ".webp" };
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedExtensions.Contains(extension))
                return BadRequest(ApiResponse<string>.ErrorResponse("Invalid file type. Allowed: JPG, PNG, GIF, WEBP"));

            // Validate file size (max 5MB)
            if (file.Length > 5 * 1024 * 1024)
                return BadRequest(ApiResponse<string>.ErrorResponse("File size exceeds 5MB limit"));

            // Create uploads directory if not exists
            var uploadsFolder = Path.Combine(_environment.WebRootPath, "uploads", "listings");
            if (!Directory.Exists(uploadsFolder))
                Directory.CreateDirectory(uploadsFolder);

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{extension}";
            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            // Return URL
            var fileUrl = $"/uploads/listings/{fileName}";
            
            _logger.LogInformation("Image uploaded successfully: {FileName}", fileName);
            
            return Ok(ApiResponse<string>.SuccessResponse(fileUrl, "Image uploaded successfully"));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading image");
            return StatusCode(500, ApiResponse<string>.ErrorResponse("Error uploading image"));
        }
    }
}
