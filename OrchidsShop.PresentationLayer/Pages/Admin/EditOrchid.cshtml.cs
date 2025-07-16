using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchidsShop.PresentationLayer.Services;
using OrchidsShop.PresentationLayer.Models.Orchids;
using OrchidsShop.PresentationLayer.Models.Categories;

namespace OrchidsShop.PresentationLayer.Pages.Admin;

public class EditOrchidModel : PageModel
{
    private readonly ILogger<EditOrchidModel> _logger;
    private readonly OrchidApiService _orchidService;
    private readonly CategoryApiService _categoryService;

    public EditOrchidModel(
        ILogger<EditOrchidModel> logger, 
        OrchidApiService orchidService, 
        CategoryApiService categoryService)
    {
        _logger = logger;
        _orchidService = orchidService;
        _categoryService = categoryService;
    }

    [BindProperty]
    public OrchidRequestModel Orchid { get; set; } = new();
    
    public OrchidModel OrchidDisplay { get; set; } = new();
    
    public List<CategoryModel> Categories { get; set; } = new();

    public async Task<IActionResult> OnGetAsync(Guid id)
    {
        try
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
            ViewData["Title"] = "Edit Orchid";

            // Load categories first
            await LoadCategoriesAsync();

            // Load orchid data
            var orchidsResponse = await _orchidService.GetOrchidsAsync(new OrchidQueryModel
            {
                Ids = id.ToString(),
                PageSize = 1
            });

            if (orchidsResponse?.Success == true && orchidsResponse.Data?.Any() == true)
            {
                var orchidData = orchidsResponse.Data.First();
                OrchidDisplay = orchidData;
                
                // Map to request model for form binding
                Orchid = new OrchidRequestModel
                {
                    Id = orchidData.Id,
                    Name = orchidData.Name,
                    Description = orchidData.Description,
                    Url = orchidData.Url,
                    Price = orchidData.Price,
                    IsNatural = orchidData.IsNatural,
                    CategoryId = orchidData.Category?.Id
                };
                
                _logger.LogInformation("Successfully loaded orchid with ID: {Id}", id);
                return Page();
            }
            else
            {
                _logger.LogWarning("Orchid not found with ID: {Id}", id);
                TempData["ErrorMessage"] = "Orchid not found.";
                return RedirectToPage("/Admin/Orchids");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading orchid for editing");
            TempData["ErrorMessage"] = "An error occurred while loading the orchid. Please try again.";
            return RedirectToPage("/Admin/Orchids");
        }
    }

    public async Task<IActionResult> OnPostAsync()
    {
        try
        {
            // Check if user is logged in and is admin
            var userRole = HttpContext.Session.GetString("UserRole");
            if (string.IsNullOrEmpty(userRole) || userRole != "ADMIN")
            {
                TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
                return RedirectToPage("/Auth/Login");
            }

            // Load categories for the form
            await LoadCategoriesAsync();

            // Validate the model
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Model validation failed: {Errors}", 
                    string.Join(", ", ModelState.Values.SelectMany(v => v.Errors).Select(e => e.ErrorMessage)));
                return Page();
            }

            // Validate required fields
            if (string.IsNullOrWhiteSpace(Orchid.Name))
            {
                ModelState.AddModelError("Orchid.Name", "Name is required");
                return Page();
            }

            if (!Orchid.CategoryId.HasValue)
            {
                ModelState.AddModelError("Orchid.CategoryId", "Category is required");
                return Page();
            }

            if (!Orchid.Price.HasValue || Orchid.Price <= 0)
            {
                ModelState.AddModelError("Orchid.Price", "Valid price is required");
                return Page();
            }

            if (!Orchid.IsNatural.HasValue)
            {
                ModelState.AddModelError("Orchid.IsNatural", "Type is required");
                return Page();
            }

            // Create request model for API
            var orchidRequest = new OrchidRequestModel
            {
                Id = Orchid.Id,
                Name = Orchid.Name,
                Description = Orchid.Description,
                Url = Orchid.Url,
                Price = Orchid.Price.Value,
                IsNatural = Orchid.IsNatural.Value,
                CategoryId = Orchid.CategoryId
            };

            // Update the orchid
            var updateResponse = await _orchidService.UpdateOrchidAsync(orchidRequest);

            if (updateResponse?.Success == true)
            {
                _logger.LogInformation("Successfully updated orchid with ID: {Id}", Orchid.Id);
                TempData["SuccessMessage"] = "Orchid updated successfully!";
                return RedirectToPage("/Admin/Orchids");
            }
            else
            {
                _logger.LogWarning("Failed to update orchid. Message: {Message}", updateResponse?.Message);
                TempData["ErrorMessage"] = updateResponse?.Message ?? "Failed to update orchid.";
                return Page();
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating orchid");
            TempData["ErrorMessage"] = "An error occurred while updating the orchid. Please try again.";
            return Page();
        }
    }

    private async Task LoadCategoriesAsync()
    {
        try
        {
            var categoriesResponse = await _categoryService.GetCategoriesAsync(new CategoryQueryModel
            {
                PageSize = 100, // Get all categories
                SortColumn = "Name",
                SortDir = "Asc"
            });

            if (categoriesResponse?.Success == true && categoriesResponse.Data != null)
            {
                Categories = categoriesResponse.Data;
                _logger.LogInformation("Successfully loaded {Count} categories", categoriesResponse.Data.Count);
            }
            else
            {
                _logger.LogWarning("Failed to load categories. Success: {Success}, Message: {Message}", 
                    categoriesResponse?.Success, categoriesResponse?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categories");
        }
    }
} 