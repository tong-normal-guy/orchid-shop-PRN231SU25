using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchidsShop.PresentationLayer.Services;
using OrchidsShop.PresentationLayer.Models.Accounts;
using System.ComponentModel.DataAnnotations;

namespace OrchidsShop.PresentationLayer.Pages.Auth;

public class LoginModel : PageModel
{
    private readonly ILogger<LoginModel> _logger;
    private readonly AccountApiService _accountService;

    public LoginModel(ILogger<LoginModel> logger, AccountApiService accountService)
    {
        _logger = logger;
        _accountService = accountService;
    }

    [BindProperty]
    public LoginRequestModel LoginRequest { get; set; } = new();

    [BindProperty]
    public bool RememberMe { get; set; }

    public string? ReturnUrl { get; set; }

    public void OnGet(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;
        
        // Clear any existing session
        HttpContext.Session.Clear();
    }

    public async Task<IActionResult> OnPostAsync(string? returnUrl = null)
    {
        ReturnUrl = returnUrl;

        if (!ModelState.IsValid)
        {
            return Page();
        }

        try
        {
            // Call the authentication API
            var response = await _accountService.LoginAsync(LoginRequest);

            if (response?.Success == true)
            {
                // Store user information in session
                HttpContext.Session.SetString("UserEmail", LoginRequest.Email ?? "");
                
                // If the login response includes user details, store them
                if (!string.IsNullOrEmpty(response.Message))
                {
                    // Assuming the message contains user info or we can parse it
                    // For now, we'll extract the name from email
                    var displayName = LoginRequest.Email?.Split('@')[0] ?? "User";
                    HttpContext.Session.SetString("UserName", displayName);
                }

                // Set session timeout based on RememberMe
                if (RememberMe)
                {
                    // Extend session for 30 days
                    HttpContext.Session.SetString("RememberMe", "true");
                }

                _logger.LogInformation($"User {LoginRequest.Email} logged in successfully");

                // Redirect to return URL or home page
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                {
                    return Redirect(returnUrl);
                }
                
                TempData["SuccessMessage"] = "Welcome back! You have been logged in successfully.";
                return RedirectToPage("/Index");
            }
            else
            {
                // Handle authentication failure
                ModelState.AddModelError(string.Empty, response?.Message ?? "Invalid email or password. Please try again.");
                TempData["ErrorMessage"] = response?.Message ?? "Invalid login credentials.";
                
                _logger.LogWarning($"Failed login attempt for {LoginRequest.Email}");
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during login for {LoginRequest.Email}");
            ModelState.AddModelError(string.Empty, "An error occurred during login. Please try again.");
            TempData["ErrorMessage"] = "Login service is temporarily unavailable. Please try again later.";
            return Page();
        }
    }
}

 