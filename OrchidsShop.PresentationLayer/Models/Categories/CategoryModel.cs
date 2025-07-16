namespace OrchidsShop.PresentationLayer.Models.Categories;

/// <summary>
/// Category response model matching QueryCategoryResponse from BLL
/// </summary>
public class CategoryModel
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
}

/// <summary>
/// Category request model for creating/updating categories
/// </summary>
public class CategoryRequestModel
{
    public string? Name { get; set; }
}

/// <summary>
/// Category query parameters for filtering and pagination
/// </summary>
public class CategoryQueryModel
{
    public string? Search { get; set; }
    public List<string>? Ids { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortColumn { get; set; }
    public string? SortDir { get; set; }
} 