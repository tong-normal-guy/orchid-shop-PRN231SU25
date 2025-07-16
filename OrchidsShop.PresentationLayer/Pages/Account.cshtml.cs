using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrchidsShop.PresentationLayer.Pages;

public class AccountModel : PageModel
{
    private readonly ILogger<AccountModel> _logger;

    public AccountModel(ILogger<AccountModel> logger)
    {
        _logger = logger;
    }

    public void OnGet()
    {
        // Account functionality will be enhanced when user management API is integrated
        // For now, this page displays session-based user information
    }
} 