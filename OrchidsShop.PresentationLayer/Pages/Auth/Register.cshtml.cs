using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchidsShop.PresentationLayer.Services;
using OrchidsShop.PresentationLayer.Models.Accounts;
using System.ComponentModel.DataAnnotations;

namespace OrchidsShop.PresentationLayer.Pages.Auth;

public class RegisterModel : PageModel
{
    private readonly ILogger<RegisterModel> _logger;
    private readonly AccountApiService _accountService;

    public RegisterModel(ILogger<RegisterModel> logger, AccountApiService accountService)
    {
        _logger = logger;
        _accountService = accountService;
    }

    [BindProperty]
    public RegisterRequestModel RegisterRequest { get; set; } = new();

    [BindProperty]
    public bool AgreeToTerms { get; set; }

    public void OnGet()
    {
        // Set default role to Customer
        RegisterRequest.Role = "Customer";
    }

    public async Task<IActionResult> OnPostAsync()
    {
        if (!ModelState.IsValid)
        {
            return Page();
        }

        if (!AgreeToTerms)
        {
            ModelState.AddModelError(nameof(AgreeToTerms), "You must agree to the Terms of Service and Privacy Policy.");
            return Page();
        }

        try
        {
            // Call the registration API
            var response = await _accountService.RegisterAsync(RegisterRequest);

            if (response?.Success == true)
            {
                _logger.LogInformation($"User {RegisterRequest.Email} registered successfully");
                
                // Registration successful, redirect to login with success message
                TempData["SuccessMessage"] = "Account created successfully! Please log in with your new credentials.";
                return RedirectToPage("/Auth/Login");
            }
            else
            {
                // Handle registration failure
                var errorMessage = response?.Message ?? "Registration failed. Please try again.";
                
                // Check if it's a specific error we can handle
                if (errorMessage.Contains("email", StringComparison.OrdinalIgnoreCase) && 
                    errorMessage.Contains("already", StringComparison.OrdinalIgnoreCase))
                {
                    ModelState.AddModelError(nameof(RegisterRequest.Email), "An account with this email address already exists.");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, errorMessage);
                }
                
                TempData["ErrorMessage"] = errorMessage;
                _logger.LogWarning($"Failed registration attempt for {RegisterRequest.Email}: {errorMessage}");
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error during registration for {RegisterRequest.Email}");
            ModelState.AddModelError(string.Empty, "An error occurred during registration. Please try again.");
            TempData["ErrorMessage"] = "Registration service is temporarily unavailable. Please try again later.";
            return Page();
        }
    }
}

 