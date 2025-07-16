using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchidsShop.PresentationLayer.Models.Orders;
using OrchidsShop.PresentationLayer.Models.Commons;
using OrchidsShop.PresentationLayer.Services;
using System.Text.Json;

namespace OrchidsShop.PresentationLayer.Pages;

public class OrdersModel : PageModel
{
    private readonly ILogger<OrdersModel> _logger;
    private readonly OrderApiService _orderService;
    private readonly AccountApiService _accountService;

    public OrdersModel(ILogger<OrdersModel> logger, OrderApiService orderService, AccountApiService accountService)
    {
        _logger = logger;
        _orderService = orderService;
        _accountService = accountService;
    }

    public List<OrderModel> Orders { get; set; } = new();
    public PaginationModel? Pagination { get; set; }
    public string? ErrorMessage { get; set; }
    public bool HasOrders => Orders.Any();
    public decimal TotalSpent => Orders.Sum(o => o.TotalAmount ?? 0);
    public int TotalOrchidsPurchased => Orders.Sum(o => o.OrderDetails?.Sum(od => od.Quantity ?? 0) ?? 0);

    // Filter properties
    [BindProperty(SupportsGet = true)]
    public string? Status { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? FromDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? ToDate { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? Search { get; set; }

    [BindProperty(SupportsGet = true)]
    public string? SortColumn { get; set; } = "OrderDate";

    [BindProperty(SupportsGet = true)]
    public string? SortDir { get; set; } = "Desc";

    [BindProperty(SupportsGet = true)]
    public int Page { get; set; } = 1;

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userId = HttpContext.Session.GetString("UserId");
            
            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userId))
            {
                return RedirectToPage("/Auth/Login");
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                ErrorMessage = "Invalid user session. Please log in again.";
                return Page();
            }

            // Create query model with filters
            var queryModel = new OrderQueryModel
            {
                AccountId = userGuid,
                PageNumber = Page,
                PageSize = 10,
                SortColumn = SortColumn,
                SortDir = SortDir
            };

            // Add filters if provided
            if (!string.IsNullOrEmpty(Status))
            {
                queryModel.Status = Status;
            }

            if (!string.IsNullOrEmpty(Search))
            {
                queryModel.Search = Search;
            }

            if (!string.IsNullOrEmpty(FromDate) && !string.IsNullOrEmpty(ToDate))
            {
                if (DateTime.TryParse(FromDate, out var fromDate) && DateTime.TryParse(ToDate, out var toDate))
                {
                    queryModel.StartDate = fromDate;
                    queryModel.EndDate = toDate;
                }
            }

            // Get user orders with filters
            var ordersResponse = await _orderService.GetOrdersAsync(queryModel);
            
            if (ordersResponse?.Success == true)
            {
                Orders = ordersResponse.Data ?? new List<OrderModel>();
                Pagination = ordersResponse.Pagination;
            }
            else
            {
                ErrorMessage = ordersResponse?.Message ?? "Failed to load orders.";
                Orders = new List<OrderModel>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading orders for user");
            ErrorMessage = "An error occurred while loading your orders.";
            Orders = new List<OrderModel>();
        }

        return Page();
    }

    /// <summary>
    /// Lấy chi tiết đơn hàng theo ID
    /// </summary>
    /// <param name="id">ID của đơn hàng</param>
    /// <returns>Chi tiết đơn hàng</returns>
    public async Task<IActionResult> OnGetOrderDetailsAsync(Guid id)
    {
        try
        {
            var userEmail = HttpContext.Session.GetString("UserEmail");
            var userId = HttpContext.Session.GetString("UserId");
            
            if (string.IsNullOrEmpty(userEmail) || string.IsNullOrEmpty(userId))
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            if (!Guid.TryParse(userId, out var userGuid))
            {
                return new JsonResult(new { success = false, message = "Invalid user session" });
            }

            // Get order details using the OrderApiService
            var orderResponse = await _orderService.GetOrderByIdAsync(id);
            
            if (orderResponse?.Success == true && orderResponse.Data?.Any() == true)
            {
                var order = orderResponse.Data.First();
                
                // Verify the order belongs to the current user
                if (order.Account?.Id != userGuid)
                {
                    return new JsonResult(new { success = false, message = "Order not found or access denied" });
                }
                
                return new JsonResult(new { success = true, data = orderResponse.Data });
            }
            else
            {
                return new JsonResult(new { success = false, message = orderResponse?.Message ?? "Order not found" });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading order details for order {OrderId}", id);
            return new JsonResult(new { success = false, message = "An error occurred while loading order details" });
        }
    }
} 