using Microsoft.EntityFrameworkCore;
using CycleTrust.Core.Entities;
using CycleTrust.Infrastructure.Data;
using CycleTrust.Application.DTOs.Catalog;
using AutoMapper;

namespace CycleTrust.Application.Services;

public interface ICatalogService
{
    Task<List<BrandDto>> GetBrandsAsync();
    Task<List<CategoryDto>> GetCategoriesAsync();
    Task<List<SizeOptionDto>> GetSizesAsync();
    Task<BrandDto> CreateBrandAsync(CreateBrandRequest request);
    Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request);
    Task<SizeOptionDto> CreateSizeAsync(CreateSizeOptionRequest request);
}

public class CatalogService : ICatalogService
{
    private readonly CycleTrustDbContext _context;
    private readonly IMapper _mapper;

    public CatalogService(CycleTrustDbContext context, IMapper mapper)
    {
        _context = context;
        _mapper = mapper;
    }

    public async Task<List<BrandDto>> GetBrandsAsync()
    {
        var brands = await _context.Brands
            .Where(b => b.IsActive)
            .OrderBy(b => b.Name)
            .ToListAsync();
        return _mapper.Map<List<BrandDto>>(brands);
    }

    public async Task<List<CategoryDto>> GetCategoriesAsync()
    {
        var categories = await _context.BikeCategories
            .Where(c => c.IsActive)
            .OrderBy(c => c.Name)
            .ToListAsync();
        return _mapper.Map<List<CategoryDto>>(categories);
    }

    public async Task<List<SizeOptionDto>> GetSizesAsync()
    {
        var sizes = await _context.SizeOptions
            .Where(s => s.IsActive)
            .OrderBy(s => s.Label)
            .ToListAsync();
        return _mapper.Map<List<SizeOptionDto>>(sizes);
    }

    public async Task<BrandDto> CreateBrandAsync(CreateBrandRequest request)
    {
        var brand = new Brand
        {
            Name = request.Name,
            IsActive = true
        };

        _context.Brands.Add(brand);
        await _context.SaveChangesAsync();

        return _mapper.Map<BrandDto>(brand);
    }

    public async Task<CategoryDto> CreateCategoryAsync(CreateCategoryRequest request)
    {
        var category = new BikeCategory
        {
            Name = request.Name,
            IsActive = true
        };

        _context.BikeCategories.Add(category);
        await _context.SaveChangesAsync();

        return _mapper.Map<CategoryDto>(category);
    }

    public async Task<SizeOptionDto> CreateSizeAsync(CreateSizeOptionRequest request)
    {
        var size = new SizeOption
        {
            Label = request.Label,
            IsActive = true
        };

        _context.SizeOptions.Add(size);
        await _context.SaveChangesAsync();

        return _mapper.Map<SizeOptionDto>(size);
    }
}
