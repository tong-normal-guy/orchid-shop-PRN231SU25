using OrchidsShop.PresentationLayer.Models.Accounts;
using OrchidsShop.PresentationLayer.Models.Orchids;

namespace OrchidsShop.PresentationLayer.Models.Orders;

/// <summary>
/// Order response model matching QueryOrderResponse from BLL
/// </summary>
public class OrderModel
{
    public Guid? Id { get; set; }
    public DateOnly? OrderDate { get; set; }
    public decimal? TotalAmount { get; set; }
    public string? Status { get; set; }
    public AccountModel? Account { get; set; }
    public IEnumerable<OrderDetailModel>? OrderDetails { get; set; }
}

/// <summary>
/// Order detail response model matching QueryOrderDetailResponse from BLL
/// </summary>
public class OrderDetailModel
{
    public Guid? Id { get; set; }
    public int? Quantity { get; set; }
    public decimal? Price { get; set; }
    public OrchidModel? Orchid { get; set; }
}

/// <summary>
/// Order request model for creating/updating orders
/// </summary>
public class OrderRequestModel
{
    public Guid? Id { get; set; }
    public Guid? AccountId { get; set; }
    public string? Status { get; set; }
    public List<OrderDetailRequestModel>? OrderDetails { get; set; }
}

/// <summary>
/// Order detail request model for creating/updating order details
/// </summary>
public class OrderDetailRequestModel
{
    public Guid? Id { get; set; }
    public Guid? OrchidId { get; set; }
    public int? Quantity { get; set; }
    public decimal? Price { get; set; }
}

/// <summary>
/// Order query parameters for filtering and pagination
/// </summary>
public class OrderQueryModel
{
    public string? Search { get; set; }
    public string? Status { get; set; }
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
    public Guid? AccountId { get; set; }
    public string? Ids { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortColumn { get; set; }
    public string? SortDir { get; set; }
} 