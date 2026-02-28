using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CycleTrust.Application.DTOs.Catalog;
using CycleTrust.Application.DTOs.Common;
using CycleTrust.Application.Services;

namespace CycleTrust.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class CatalogController : ControllerBase
{
    private readonly ICatalogService _catalogService;

    public CatalogController(ICatalogService catalogService)
    {
        _catalogService = catalogService;
    }

    [HttpGet("brands")]
    public async Task<ActionResult<ApiResponse<List<BrandDto>>>> GetBrands()
    {
        try
        {
            var brands = await _catalogService.GetBrandsAsync();
            return Ok(ApiResponse<List<BrandDto>>.SuccessResponse(brands));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<BrandDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("categories")]
    public async Task<ActionResult<ApiResponse<List<CategoryDto>>>> GetCategories()
    {
        try
        {
            var categories = await _catalogService.GetCategoriesAsync();
            return Ok(ApiResponse<List<CategoryDto>>.SuccessResponse(categories));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<CategoryDto>>.ErrorResponse(ex.Message));
        }
    }

    [HttpGet("sizes")]
    public async Task<ActionResult<ApiResponse<List<SizeOptionDto>>>> GetSizes()
    {
        try
        {
            var sizes = await _catalogService.GetSizesAsync();
            return Ok(ApiResponse<List<SizeOptionDto>>.SuccessResponse(sizes));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<List<SizeOptionDto>>.ErrorResponse(ex.Message));
        }
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("brands")]
    public async Task<ActionResult<ApiResponse<BrandDto>>> CreateBrand([FromBody] CreateBrandRequest request)
    {
        try
        {
            var brand = await _catalogService.CreateBrandAsync(request);
            return Ok(ApiResponse<BrandDto>.SuccessResponse(brand, "Tạo thương hiệu thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<BrandDto>.ErrorResponse(ex.Message));
        }
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("categories")]
    public async Task<ActionResult<ApiResponse<CategoryDto>>> CreateCategory([FromBody] CreateCategoryRequest request)
    {
        try
        {
            var category = await _catalogService.CreateCategoryAsync(request);
            return Ok(ApiResponse<CategoryDto>.SuccessResponse(category, "Tạo danh mục thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<CategoryDto>.ErrorResponse(ex.Message));
        }
    }

    [Authorize(Roles = "ADMIN")]
    [HttpPost("sizes")]
    public async Task<ActionResult<ApiResponse<SizeOptionDto>>> CreateSize([FromBody] CreateSizeOptionRequest request)
    {
        try
        {
            var size = await _catalogService.CreateSizeAsync(request);
            return Ok(ApiResponse<SizeOptionDto>.SuccessResponse(size, "Tạo size thành công"));
        }
        catch (Exception ex)
        {
            return BadRequest(ApiResponse<SizeOptionDto>.ErrorResponse(ex.Message));
        }
    }
}
