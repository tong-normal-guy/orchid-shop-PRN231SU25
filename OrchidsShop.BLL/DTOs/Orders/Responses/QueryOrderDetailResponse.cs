using OrchidsShop.BLL.DTOs.Orchids.Responses;

namespace OrchidsShop.BLL.DTOs.Orders.Responses;

public class QueryOrderDetailResponse
{
    public Guid? Id { get; set; }
    public int? Quantity { get; set; }
    public decimal? Price { get; set; }
    public QueryOrchidResponse Orchid { get; set; }
}