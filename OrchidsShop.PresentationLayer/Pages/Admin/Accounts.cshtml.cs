using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchidsShop.PresentationLayer.Models.Accounts;
using OrchidsShop.PresentationLayer.Services;

namespace OrchidsShop.PresentationLayer.Pages.Admin;

public class AccountsModel : PageModel
{
    private readonly ILogger<AccountsModel> _logger;
    private readonly AccountApiService _accountService;

    public List<AccountModel> Accounts { get; set; } = new();
    public List<string> Roles { get; set; } = new();
    public int TotalAccounts { get; set; }
    public int AdminAccounts { get; set; }
    public int StaffAccounts { get; set; }
    public int CustomerAccounts { get; set; }

    public AccountsModel(ILogger<AccountsModel> logger, AccountApiService accountService)
    {
        _logger = logger;
        _accountService = accountService;
    }

    public async Task<IActionResult> OnGetAsync()
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
            await LoadAccountsAsync();
            await LoadRolesAsync();
            await LoadStatisticsAsync();
            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading accounts for admin");
            TempData["ErrorMessage"] = "An error occurred while loading accounts. Please try again.";
            return Page();
        }
    }

    private async Task LoadAccountsAsync()
    {
        try
        {
            _logger.LogInformation("Loading accounts from API...");
            
            // Use the real API to get all accounts
            var response = await _accountService.GetAllAccountsAsync();
            
            _logger.LogInformation("API Response received. Success: {Success}, Message: {Message}, Data Count: {DataCount}", 
                response?.Success, response?.Message, response?.Data?.Count ?? 0    )  ;
            if (response?.Success == true && response.Data != null)
            {
                Accounts = response.Data;
                _logger.LogInformation("Successfully loaded {Count} accounts", Accounts.Count);
                
                // Log first few accounts for debugging
                foreach (var account in Accounts.Take(3))
                {
                    _logger.LogInformation("Account: ID={Id}, Name={Name}, Email={Email}, Role={Role}", 
                        account.Id, account.Name, account.Email, account.Role);
                }
            }
            else
            {
                _logger.LogWarning("Failed to load accounts from API. Success: {Success}, Message: {Message}, Errors: {Errors}", 
                    response?.Success, response?.Message, response?.Errors != null ? string.Join(", ", response.Errors) : "None");
                Accounts = new List<AccountModel>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading accounts from API");
            Accounts = new List<AccountModel>();
        }
    }

    private async Task LoadRolesAsync()
    {
        try
        {
            _logger.LogInformation("Loading roles from API...");
            var response = await _accountService.GetRolesAsync();
            _logger.LogInformation("API Response received. Success: {Success}, Message: {Message}, Data Count: {DataCount}", 
                response?.Success, response?.Message, response?.Data?.Count ?? 0);
            if (response?.Success == true && response.Data != null)
            {
                Roles = response.Data;
                _logger.LogInformation("Successfully loaded {Count} roles", Roles.Count);
            }
            else
            {
                _logger.LogWarning("Failed to load roles from API. Success: {Success}, Message: {Message}, Errors: {Errors}", 
                    response?.Success, response?.Message, response?.Errors != null ? string.Join(", ", response.Errors) : "None");
                Roles = new List<string>();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading roles from API");
            Roles = new List<string>();
        }
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            // Statistics are now calculated from the loaded accounts
            TotalAccounts = Accounts.Count;
            AdminAccounts = Accounts.Count(a => a.Role?.ToUpper() == "ADMIN");
            StaffAccounts = Accounts.Count(a => a.Role?.ToUpper() == "STAFF");
            CustomerAccounts = Accounts.Count(a => a.Role?.ToUpper() == "CUSTOMER");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading account statistics");
            TotalAccounts = 0;
            AdminAccounts = 0;
            StaffAccounts = 0;
            CustomerAccounts = 0;
        }
    }
} 