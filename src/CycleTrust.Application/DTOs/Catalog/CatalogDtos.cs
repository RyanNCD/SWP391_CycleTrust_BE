namespace CycleTrust.Application.DTOs.Catalog;

public class BrandDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateBrandRequest
{
    public string Name { get; set; } = string.Empty;
}

public class CategoryDto
{
    public long Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateCategoryRequest
{
    public string Name { get; set; } = string.Empty;
}

public class SizeOptionDto
{
    public long Id { get; set; }
    public string Label { get; set; } = string.Empty;
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateSizeOptionRequest
{
    public string Label { get; set; } = string.Empty;
}
