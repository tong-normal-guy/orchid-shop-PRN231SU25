using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchidsShop.PresentationLayer.Models.Orders;
using OrchidsShop.PresentationLayer.Services;

namespace OrchidsShop.PresentationLayer.Pages.Orders;

public class DetailsModel : PageModel
{
    private readonly OrderApiService _orderService;
    private readonly ILogger<DetailsModel> _logger;

    public DetailsModel(OrderApiService orderService, ILogger<DetailsModel> logger)
    {
        _orderService = orderService;
        _logger = logger;
    }

    [BindProperty(SupportsGet = true)]
    public Guid Id { get; set; }

    public OrderModel? Order { get; set; }
    public string? ErrorMessage { get; set; }

    public async Task<IActionResult> OnGetAsync()
    {
        try
        {
            // Check if user is logged in
            var userEmail = HttpContext.Session.GetString("UserEmail");
            if (string.IsNullOrEmpty(userEmail))
            {
                return Page();
            }

            // Validate order ID
            if (Id == Guid.Empty)
            {
                ErrorMessage = "Invalid order ID provided.";
                return Page();
            }

            // Fetch order details from API
            var response = await _orderService.GetOrderByIdAsync(Id);
            
            if (response?.Success == true && response.Data?.Any() == true)
            {
                Order = response.Data.First();
                
                // Additional validation: ensure user can only view their own orders
                // This would require getting the current user's ID from session or JWT token
                // For now, we'll allow viewing any order (admin functionality)
                
                return Page();
            }
            else
            {
                ErrorMessage = response?.Message ?? "Order not found or you don't have permission to view it.";
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching order details for order ID: {OrderId}", Id);
            ErrorMessage = "An error occurred while fetching order details. Please try again later.";
            return Page();
        }
    }
} 