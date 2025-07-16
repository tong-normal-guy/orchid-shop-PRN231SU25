using OrchidsShop.PresentationLayer.Models.Categories;

namespace OrchidsShop.PresentationLayer.Models.Orchids;

/// <summary>
/// Orchid response model matching QueryOrchidResponse from BLL
/// </summary>
public class OrchidModel
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public decimal? Price { get; set; }
    public bool? IsNatural { get; set; }
    public CategoryModel? Category { get; set; }
}

/// <summary>
/// Orchid request model for creating/updating orchids
/// </summary>
public class OrchidRequestModel
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public decimal? Price { get; set; }
    public bool? IsNatural { get; set; }
    public Guid? CategoryId { get; set; }
}

/// <summary>
/// Orchid query parameters for filtering and pagination
/// </summary>
public class OrchidQueryModel
{
    public string? Search { get; set; }
    public bool? IsNarutal { get; set; }
    public string? Categories { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string? Ids { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortColumn { get; set; }
    public string? SortDir { get; set; }
} 