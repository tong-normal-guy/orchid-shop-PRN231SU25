using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace OrchidsShop.PresentationLayer.Pages.Auth;

public class LogoutModel : PageModel
{
    private readonly ILogger<LogoutModel> _logger;

    public LogoutModel(ILogger<LogoutModel> logger)
    {
        _logger = logger;
    }

    public IActionResult OnGet()
    {
        return OnPost();
    }

    public IActionResult OnPost()
    {
        try
        {
            // Get user email before clearing session for logging
            var userEmail = HttpContext.Session.GetString("UserEmail");
            
            // Clear all session data
            HttpContext.Session.Clear();
            
            // Log the logout
            if (!string.IsNullOrEmpty(userEmail))
            {
                _logger.LogInformation($"User {userEmail} logged out successfully");
            }
            
            // Set success message
            TempData["SuccessMessage"] = "You have been logged out successfully.";
            
            // Redirect to home page
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            TempData["ErrorMessage"] = "An error occurred during logout.";
            return RedirectToPage("/Index");
        }
    }
} 