using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrchidsShop.PresentationLayer.Pages;

public class CartModel : PageModel
{
    private readonly ILogger<CartModel> _logger;

    public CartModel(ILogger<CartModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        // Cart functionality is handled client-side with localStorage
        // This page model is mainly for routing and potential server-side cart operations
    }
} 