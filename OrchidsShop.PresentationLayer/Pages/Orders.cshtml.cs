using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrchidsShop.PresentationLayer.Pages;

public class OrdersModel : PageModel
{
    private readonly ILogger<OrdersModel> _logger;

    public OrdersModel(ILogger<OrdersModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        // Order functionality will be implemented when order API is ready
        // For now, this page shows demo orders for logged-in users
    }
} 