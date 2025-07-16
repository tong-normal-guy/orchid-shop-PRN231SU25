using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchidsShop.PresentationLayer.Models.Accounts;
using OrchidsShop.PresentationLayer.Services;
using OrchidsShop.PresentationLayer.Models.Commons;
using System.Text.Json;
using OrchidsShop.PresentationLayer.Constants;

namespace OrchidsShop.PresentationLayer.Pages;

public class AccountPageModel : PageModel
{
    private readonly ILogger<AccountPageModel> _logger;
    private readonly AccountApiService _accountApiService;

    public AccountPageModel(ILogger<AccountPageModel> logger, AccountApiService accountApiService)
    {
        _logger = logger;
        _accountApiService = accountApiService;
    }

    public AccountModel? Account { get; set; }
    public bool IsLoaded { get; set; } = false;
    public string? ErrorMessage { get; set; }

    public async Task OnGetAsync()
    {
        // Check if user has JWT token in session
        var jwtToken = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(jwtToken))
        {
            _logger.LogWarning("User attempted to access account page without JWT token");
            ErrorMessage = "Please log in to view your account details.";
            return;
        }

        _logger.LogInformation("Attempting to load current user profile with JWT token");
        
        try
        {
            var response = await _accountApiService.GetCurrentProfileAsync();
            
            // Temporary debugging
            _logger.LogInformation($"API Response - Success: {response?.Success}, Message: {response?.Message}");
            if (response?.Data != null)
            {
                _logger.LogInformation($"API Response - Data Count: {response.Data.Count}");
                foreach (var item in response.Data)
                {
                    _logger.LogInformation($"Account Data - ID: {item.Id}, Name: {item.Name}, Email: {item.Email}, Role: {item.Role}");
                }
            }
            
            if (response?.Success == true && response.Data != null && response.Data.Count > 0)
            {
                Account = response.Data.FirstOrDefault();
                IsLoaded = true;
                _logger.LogInformation($"Successfully loaded profile for user: {Account?.Email}");
            }
            else
            {
                var errorMsg = response?.Message ?? "Failed to load account details.";
                _logger.LogWarning($"Failed to load profile. API Response: {errorMsg}");
                ErrorMessage = errorMsg;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while loading user profile");
            ErrorMessage = "An error occurred while loading your account details. Please try again.";
        }
    }

    public async Task<IActionResult> OnPostUpdateProfileAsync()
    {
        try
        {
            // Check if user has JWT token
            var jwtToken = HttpContext.Session.GetString("JwtToken");
            if (string.IsNullOrEmpty(jwtToken))
            {
                return new JsonResult(new { success = false, message = "User not authenticated" });
            }

            // Read request body
            using var reader = new StreamReader(Request.Body);
            var requestBody = await reader.ReadToEndAsync();
            var updateRequest = JsonSerializer.Deserialize<UpdateProfileRequest>(requestBody);

            if (updateRequest == null)
            {
                return new JsonResult(new { success = false, message = "Invalid request data" });
            }

            // Basic validation
            if (string.IsNullOrWhiteSpace(updateRequest.Name) || string.IsNullOrWhiteSpace(updateRequest.Email))
            {
                return new JsonResult(new { success = false, message = "Name and email are required" });
            }

            // Get current user profile to get the ID
            var currentProfileResponse = await _accountApiService.GetCurrentProfileAsync();
            if (currentProfileResponse?.Success != true || currentProfileResponse.Data == null || !currentProfileResponse.Data.Any())
            {
                return new JsonResult(new { success = false, message = "Failed to get current user profile" });
            }

            var currentUser = currentProfileResponse.Data.First();
            var isAdmin = currentUser.Role?.Equals(EnumAccountRole.ADMIN.ToString(), StringComparison.OrdinalIgnoreCase) == true;

            // Create update request
            var accountUpdateRequest = new AccountRequestModel
            {
                Id = currentUser.Id,
                Name = updateRequest.Name,
                Email = updateRequest.Email
                // Note: Role is not included - only admins can change roles
            };

            // Call API to update profile
            var updateResponse = await _accountApiService.UpdateAccountAsync(currentUser.Id!.Value, accountUpdateRequest);

            if (updateResponse?.Success == true)
            {
                // Update session data
                HttpContext.Session.SetString("UserName", updateRequest.Name);
                HttpContext.Session.SetString("UserEmail", updateRequest.Email);

                return new JsonResult(new { 
                    success = true, 
                    message = "Profile updated successfully!" 
                });
            }
            else
            {
                return new JsonResult(new { 
                    success = false, 
                    message = updateResponse?.Message ?? "Failed to update profile" 
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception occurred while updating profile");
            return new JsonResult(new { 
                success = false, 
                message = "An error occurred while updating profile" 
            });
        }
    }
}

// Request model for profile updates
public class UpdateProfileRequest
{
    public string? Name { get; set; }
    public string? Email { get; set; }
} 