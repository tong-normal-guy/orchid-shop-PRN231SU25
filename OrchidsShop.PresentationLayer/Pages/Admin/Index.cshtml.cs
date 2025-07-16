using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrchidsShop.PresentationLayer.Pages.Admin;

public class IndexModel : PageModel
{
    public IActionResult OnGet()
    {
        // Check if user is logged in and is admin
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || userRole != "ADMIN")
        {
            TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
            return RedirectToPage("/Auth/Login");
        }

        // Set ViewData for active page
        ViewData["ActivePage"] = "Admin";
        ViewData["Title"] = "Admin Dashboard";

        return Page();
    }
} 