using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchidsShop.PresentationLayer.Models.Orders;
using OrchidsShop.PresentationLayer.Services;
using System.Security.Claims;

namespace OrchidsShop.PresentationLayer.Pages.Admin;

public class OrdersModel : PageModel
{
    private readonly ILogger<OrdersModel> _logger;
    private readonly OrderApiService _orderService;

    public List<OrderModel> Orders { get; set; } = new();
    public int TotalOrders { get; set; }
    public int PendingOrders { get; set; }
    public int PaidOrders { get; set; }
    public int CancelledOrders { get; set; }

    public OrdersModel(ILogger<OrdersModel> logger, OrderApiService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public async Task<IActionResult> OnGetAsync()
    {
        // Check admin privileges
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || userRole != "ADMIN")
        {
            TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
            return RedirectToPage("/Auth/Login");
        }

        try
        {
            await LoadOrdersAsync();
            await LoadStatisticsAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading orders for admin");
            TempData["ErrorMessage"] = "An error occurred while loading orders. Please try again.";
            return Page();
        }
    }

    public async Task<IActionResult> OnPostUpdateStatusAsync(string orderId, string status)
    {
        // Check admin privileges
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || userRole != "ADMIN")
        {
            return new JsonResult(new { success = false, message = "Access denied. Admin privileges required." });
        }

        try
        {
            if (!Guid.TryParse(orderId, out var orderGuid))
            {
                return new JsonResult(new { success = false, message = "Invalid order ID format." });
            }

            // Create update request
            var updateRequest = new OrderRequestModel
            {
                Id = orderGuid,
                Status = status
            };

            var response = await _orderService.UpdateOrderAsync(updateRequest);

            if (response?.Success == true)
            {
                _logger.LogInformation("Successfully updated order status. Order ID: {OrderId}, New Status: {Status}", orderId, status);
                return new JsonResult(new { success = true, message = "Order status updated successfully!" });
            }
            else
            {
                _logger.LogWarning("Failed to update order status. Order ID: {OrderId}, Status: {Status}, Message: {Message}", 
                    orderId, status, response?.Message);
                return new JsonResult(new { success = false, message = response?.Message ?? "Failed to update order status." });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating order status. Order ID: {OrderId}, Status: {Status}", orderId, status);
            return new JsonResult(new { success = false, message = "An error occurred while updating the order status." });
        }
    }

    private async Task LoadOrdersAsync()
    {
        try
        {
            var response = await _orderService.GetOrdersForAdminAsync(new OrderQueryModel
            {
                PageSize = 100, // Get all orders for admin
                SortColumn = "OrderDate",
                SortDir = "Desc"
            });

            if (response?.Success == true && response.Data != null)
            {
                Orders = response.Data;
            }
            else
            {
                _logger.LogWarning("Failed to load orders. Message: {Message}", response?.Message);
                Orders = new List<OrderModel>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading orders");
            Orders = new List<OrderModel>();
        }
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            // Load all orders to calculate statistics
            var response = await _orderService.GetOrdersForAdminAsync(new OrderQueryModel
            {
                PageSize = 1000 // Get all orders for statistics
            });

            if (response?.Success == true && response.Data != null)
            {
                var orders = response.Data;
                TotalOrders = orders.Count;
                PendingOrders = orders.Count(o => o.Status?.ToUpper() == "PENDING");
                PaidOrders = orders.Count(o => o.Status?.ToUpper() == "PAID");
                CancelledOrders = orders.Count(o => o.Status?.ToUpper() == "CANCELLED");
            }
            else
            {
                TotalOrders = 0;
                PendingOrders = 0;
                PaidOrders = 0;
                CancelledOrders = 0;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading order statistics");
            TotalOrders = 0;
            PendingOrders = 0;
            PaidOrders = 0;
            CancelledOrders = 0;
        }
    }
} 