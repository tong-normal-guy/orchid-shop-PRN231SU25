using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchidsShop.PresentationLayer.Models.Accounts;
using OrchidsShop.PresentationLayer.Services;

namespace OrchidsShop.PresentationLayer.Pages.Admin;

public class EditAccountModel : PageModel
{
    private readonly ILogger<EditAccountModel> _logger;
    private readonly AccountApiService _accountService;

    [BindProperty]
    public AccountRequestModel Account { get; set; } = new();

    public EditAccountModel(ILogger<EditAccountModel> logger, AccountApiService accountService)
    {
        _logger = logger;
        _accountService = accountService;
    }

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        // Check admin privileges
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || userRole != "ADMIN")
        {
            TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
            return RedirectToPage("/Auth/Login");
        }

        try
        {
            // Load account data
            var response = await _accountService.GetAccountByIdAsync(id);
            
            if (response?.Success == true && response.Data?.Any() == true)
            {
                var accountData = response.Data.First();
                
                // Map to request model for form binding
                Account = new AccountRequestModel
                {
                    Id = accountData.Id,
                    Name = accountData.Name,
                    Email = accountData.Email,
                    Role = accountData.Role
                };
                
                return Page();
            }
            
            TempData["ErrorMessage"] = "Account not found.";
            return RedirectToPage("/Admin/Accounts");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading account for editing. Account ID: {AccountId}", id);
            TempData["ErrorMessage"] = "An error occurred while loading the account. Please try again.";
            return RedirectToPage("/Admin/Accounts");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        // Check admin privileges
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || userRole != "ADMIN")
        {
            TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
            return RedirectToPage("/Auth/Login");
        }

        try
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            if (!Account.Id.HasValue)
            {
                TempData["ErrorMessage"] = "Invalid account ID.";
                return Page();
            }

            // Validate password confirmation if password is provided
            if (!string.IsNullOrEmpty(Account.Password) && Account.Password != Account.ConfirmPassword)
            {
                ModelState.AddModelError("Account.ConfirmPassword", "Password and confirm password do not match.");
                return Page();
            }

            // If password is empty, remove it from the request to keep current password
            if (string.IsNullOrEmpty(Account.Password))
            {
                Account.Password = null;
                Account.ConfirmPassword = null;
            }

            var response = await _accountService.UpdateAccountAsync(Account.Id.Value, Account);

            if (response?.Success == true)
            {
                _logger.LogInformation("Successfully updated account. Account ID: {AccountId}", Account.Id);
                TempData["SuccessMessage"] = "Account updated successfully!";
                return RedirectToPage("/Admin/Accounts");    
            }
            else
            {
                _logger.LogWarning("Failed to update account. Account ID: {AccountId}, Message: {Message}", 
                    Account.Id, response?.Message);
                TempData["ErrorMessage"] = response?.Message ?? "Failed to update account.";
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating account. Account ID: {AccountId}", Account.Id);
            TempData["ErrorMessage"] = "An error occurred while updating the account. Please try again.";
            return Page();
        }
    }
} 