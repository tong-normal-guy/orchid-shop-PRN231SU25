using OrchidsShop.BLL.DTOs.Categories.Responses;

namespace OrchidsShop.BLL.DTOs.Orchids.Responses;

public class QueryOrchidResponse
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public string? Url { get; set; }
    public decimal? Price { get; set; }
    public bool? IsNatural { get; set; }
    public QueryCategoryResponse Category { get; set; }
}