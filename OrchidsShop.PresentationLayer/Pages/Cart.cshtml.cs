using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchidsShop.PresentationLayer.Services;
using OrchidsShop.PresentationLayer.Models.Orders;
using OrchidsShop.PresentationLayer.Constants;

namespace OrchidsShop.PresentationLayer.Pages;

public class CartModel : PageModel
{
    private readonly ILogger<CartModel> _logger;
    private readonly OrderApiService _orderService;

    public CartModel(ILogger<CartModel> logger, OrderApiService orderService)
    {
        _logger = logger;
        _orderService = orderService;
    }

    public void OnGet()
    {
        // Cart functionality is handled client-side with localStorage
        // This page model is mainly for routing and potential server-side cart operations
    }

    /// <summary>
    /// Handle checkout and create order
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> OnPostCreateOrderAsync([FromBody] CreateOrderRequest request)
    {
        try
        {
            // Get current logged-in user ID from session
            var userIdString = HttpContext.Session.GetString("UserId");
            var userEmail = HttpContext.Session.GetString("UserEmail");
            
            if (string.IsNullOrEmpty(userIdString) || string.IsNullOrEmpty(userEmail))
            {
                return new JsonResult(new { success = false, message = "User not logged in" });
            }

            if (!Guid.TryParse(userIdString, out Guid userId))
            {
                return new JsonResult(new { success = false, message = "Invalid user session" });
            }

            // Validate request
            if (request.OrderDetails == null || !request.OrderDetails.Any())
            {
                return new JsonResult(new { success = false, message = "Cart is empty" });
            }

            // Create order request
            var orderRequest = new OrderRequestModel
            {
                AccountId = userId, // Include account ID as required by API
                Status = EnumOrderStatus.Paid.ToString(), // Use enum for status
                OrderDetails = request.OrderDetails.Select(detail => new OrderDetailRequestModel
                {
                    OrchidId = detail.OrchidId,
                    Quantity = detail.Quantity,
                    Price = detail.Price
                }).ToList()
            };

            // Create the order
            var result = await _orderService.CreateOrderAsync(orderRequest);

            if (result?.Success == true)
            {
                _logger.LogInformation($"Order created successfully for user {userEmail}");
                
                return new JsonResult(new { 
                    success = true, 
                    message = "Order created successfully! Payment processed."
                });
            }
            else
            {
                _logger.LogWarning($"Failed to create order for user {userEmail}. Message: {result?.Message}");
                
                return new JsonResult(new { 
                    success = false, 
                    message = result?.Message ?? "Failed to create order"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order during checkout");
            return new JsonResult(new { 
                success = false, 
                message = "An error occurred while processing your order" 
            });
        }
    }
}

/// <summary>
/// Request model for creating order from cart
/// </summary>
public class CreateOrderRequest
{
    public List<CreateOrderDetailRequest>? OrderDetails { get; set; }
}

/// <summary>
/// Order detail request for cart checkout
/// </summary>
public class CreateOrderDetailRequest
{
    public Guid OrchidId { get; set; }
    public int Quantity { get; set; }
    public decimal Price { get; set; }
} 