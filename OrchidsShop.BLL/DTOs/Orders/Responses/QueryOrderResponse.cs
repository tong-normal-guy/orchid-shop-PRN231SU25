using OrchidsShop.BLL.DTOs.Accounts.Responses;

namespace OrchidsShop.BLL.DTOs.Orders.Responses;

public class QueryOrderResponse
{
    public Guid? Id { get; set; }
    public DateOnly? OrderDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public QueryAccountResponse Account { get; set; }
    public IEnumerable<QueryOrderDetailResponse> OrderDetails { get; set; }
}