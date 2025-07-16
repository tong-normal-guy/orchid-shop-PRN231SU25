using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchidsShop.PresentationLayer.Services;
using OrchidsShop.PresentationLayer.Models.Orchids;
using OrchidsShop.PresentationLayer.Models.Categories;
using OrchidsShop.PresentationLayer.Models.Orders;
using OrchidsShop.PresentationLayer.Models.Commons;

namespace OrchidsShop.PresentationLayer.Pages.Admin;

public class OrchidsModel : PageModel
{
    private readonly ILogger<OrchidsModel> _logger;
    private readonly OrchidApiService _orchidService;
    private readonly CategoryApiService _categoryService;
    private readonly OrderApiService _orderService;
    private readonly AccountApiService _accountService;

    public OrchidsModel(
        ILogger<OrchidsModel> logger, 
        OrchidApiService orchidService, 
        CategoryApiService categoryService,
        OrderApiService orderService,
        AccountApiService accountService)
    {
        _logger = logger;
        _orchidService = orchidService;
        _categoryService = categoryService;
        _orderService = orderService;
        _accountService = accountService;
    }

    // Properties for the view
    public List<OrchidModel> Orchids { get; set; } = new();
    public List<CategoryModel> Categories { get; set; } = new();
    public int TotalOrchids { get; set; }
    public int TotalCategories { get; set; }
    public int TotalOrders { get; set; }
    public int TotalCustomers { get; set; }

    // Form properties for orchid creation/editing
    [BindProperty]
    public OrchidRequestModel OrchidRequest { get; set; } = new();

    public async Task<IActionResult> OnGetAsync()
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
            ViewData["Title"] = "Admin - Manage Orchids";

            // Load all data in parallel
            var tasks = new[]
            {
                LoadOrchidsAsync(),
                LoadCategoriesAsync(),
                LoadStatisticsAsync()
            };

            await Task.WhenAll(tasks);

            return Page();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading admin orchids page");
            TempData["ErrorMessage"] = "An error occurred while loading the admin page. Please try again.";
            return RedirectToPage("/Index");
        }
    }

    private async Task LoadOrchidsAsync()
    {
        try
        {
            var orchidsResponse = await _orchidService.GetOrchidsAsync(new OrchidQueryModel
            {
                PageSize = 100, // Get all orchids for admin view
                SortColumn = "Name",
                SortDir = "Asc"
            });

            if (orchidsResponse?.Success == true && orchidsResponse.Data != null)
            {
                Orchids = orchidsResponse.Data;
                _logger.LogInformation("Successfully loaded {Count} orchids for admin", orchidsResponse.Data.Count);
            }
            else
            {
                _logger.LogWarning("Failed to load orchids. Success: {Success}, Message: {Message}", 
                    orchidsResponse?.Success, orchidsResponse?.Message);
                TempData["ErrorMessage"] = $"Failed to load orchids: {orchidsResponse?.Message}";
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading orchids for admin");
            TempData["ErrorMessage"] = "Failed to load orchids.";
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
                _logger.LogInformation("Successfully loaded {Count} categories for admin", categoriesResponse.Data.Count);
            }
            else
            {
                _logger.LogWarning("Failed to load categories. Success: {Success}, Message: {Message}", 
                    categoriesResponse?.Success, categoriesResponse?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categories for admin");
        }
    }

    private async Task LoadStatisticsAsync()
    {
        try
        {
            // Load statistics in parallel
            var orchidsTask = _orchidService.GetOrchidsAsync(new OrchidQueryModel { PageSize = 1 });
            var categoriesTask = _categoryService.GetCategoriesAsync(new CategoryQueryModel { PageSize = 1 });
            var ordersTask = _orderService.GetOrdersAsync(new OrderQueryModel { PageSize = 1 });

            await Task.WhenAll(orchidsTask, categoriesTask, ordersTask);

            // Set statistics
            if (orchidsTask.Result?.Pagination != null)
                TotalOrchids = orchidsTask.Result.Pagination.TotalRecords;

            if (categoriesTask.Result?.Pagination != null)
                TotalCategories = categoriesTask.Result.Pagination.TotalRecords;

            if (ordersTask.Result?.Pagination != null)
                TotalOrders = ordersTask.Result.Pagination.TotalRecords;

            // For now, set a placeholder for customers (would need account service)
            TotalCustomers = 25; // Placeholder
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading statistics for admin");
            // Set default values
            TotalOrchids = 0;
            TotalCategories = 0;
            TotalOrders = 0;
            TotalCustomers = 0;
        }
    }


} 