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
        return ProcessLogout();
    }

    public IActionResult OnPost()
    {
        return ProcessLogout();
    }

    private IActionResult ProcessLogout()
    {
        try
        {
            // Get user info before clearing session for logging
            var userEmail = HttpContext.Session.GetString("UserEmail") ?? "Unknown";
            var userName = HttpContext.Session.GetString("UserName") ?? "User";
            
            // Clear all session data
            HttpContext.Session.Clear();
            
            // Log the logout
            _logger.LogInformation($"User {userEmail} logged out successfully");
            
            // Set success message with personalized greeting
            TempData["SuccessMessage"] = $"Goodbye {userName}! You have been logged out successfully.";
            
            // Redirect directly to home page
            return RedirectToPage("/Index");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error during logout");
            TempData["ErrorMessage"] = "An error occurred during logout, but you have been signed out.";
            return RedirectToPage("/Index");
        }
    }
} 