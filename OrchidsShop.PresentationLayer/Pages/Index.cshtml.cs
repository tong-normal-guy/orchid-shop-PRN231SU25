using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using OrchidsShop.PresentationLayer.Services;
using OrchidsShop.PresentationLayer.Models.Orchids;
using OrchidsShop.PresentationLayer.Models.Categories;
using OrchidsShop.PresentationLayer.Models.Commons;

namespace OrchidsShop.PresentationLayer.Pages;

public class IndexModel : PageModel
{
    private readonly ILogger<IndexModel> _logger;
    private readonly OrchidApiService _orchidService;
    private readonly CategoryApiService _categoryService;

    public IndexModel(ILogger<IndexModel> logger, OrchidApiService orchidService, CategoryApiService categoryService)
    {
        _logger = logger;
        _orchidService = orchidService;
        _categoryService = categoryService;
    }

    // Properties for the view
    public List<OrchidModel> Orchids { get; set; } = new();
    public List<CategoryModel> Categories { get; set; } = new();
    public PaginationModel? Pagination { get; set; }
    public string? CurrentSearch { get; set; }
    public string? CurrentCategory { get; set; }
    public bool? IsNatural { get; set; }
    public decimal? MinPrice { get; set; }
    public decimal? MaxPrice { get; set; }
    public string SortColumn { get; set; } = "Name";
    public string SortDirection { get; set; } = "Asc";
    public int CurrentPage { get; set; } = 1;
    public bool ShowProductView { get; set; } = false;

    public async Task<IActionResult> OnGetAsync(
        string? search = null,
        string? category = null,
        bool? isNatural = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int page = 1,
        string sortBy = "Name",
        string sortDir = "Asc",
        string? view = null)
    {
        try
        {
            // Set ViewData for active page
            ViewData["ActivePage"] = "Home";
            ViewData["Title"] = "Home";

            // Determine if we should show product view
            ShowProductView = !string.IsNullOrEmpty(view) && view.ToLower() == "products";
            
            if (ShowProductView)
            {
                ViewData["ActivePage"] = "Products";
                ViewData["Title"] = "Products";
            }

            // Store current filter values
            CurrentSearch = search;
            CurrentCategory = category;
            IsNatural = isNatural;
            MinPrice = minPrice;
            MaxPrice = maxPrice;
            SortColumn = sortBy;
            SortDirection = sortDir;
            CurrentPage = page;

            // Always load categories for filter dropdown
            var categoriesResponse = await _categoryService.GetCategoriesAsync(new CategoryQueryModel
            {
                PageSize = 100, // Get all categories for dropdown
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
                _logger.LogWarning("Failed to load categories. Success: {Success}, Message: {Message}, Errors: {Errors}", 
                    categoriesResponse?.Success, categoriesResponse?.Message, 
                    categoriesResponse?.Errors != null ? string.Join(", ", categoriesResponse.Errors) : "None");
                TempData["ErrorMessage"] = $"Failed to load categories: {categoriesResponse?.Message}";
            }

            // Always load orchids - either with filters or default list
            ApiResponse<List<OrchidModel>>? orchidsResponse;

            if (ShowProductView || !string.IsNullOrEmpty(search) || !string.IsNullOrEmpty(category) || 
                isNatural.HasValue || minPrice.HasValue || maxPrice.HasValue)
            {
                // Apply filters for product view
                orchidsResponse = await _orchidService.AdvancedSearchOrchidsAsync(
                    search: search,
                    categories: category,
                    isNatural: isNatural,
                    minPrice: minPrice,
                    maxPrice: maxPrice,
                    pageNumber: page,
                    pageSize: 12,
                    sortBy: sortBy,
                    sortDirection: sortDir
                );
            }
            else
            {
                // Show all orchids for home page (not just featured)
                orchidsResponse = await _orchidService.GetOrchidsForCatalogAsync(
                    pageNumber: page,
                    pageSize: 12,  // Show more orchids on home page
                    sortBy: "Name",
                    sortDirection: "Asc"
                );
            }

            if (orchidsResponse?.Success == true && orchidsResponse.Data != null)
            {
                Orchids = orchidsResponse.Data;
                Pagination = orchidsResponse.Pagination;
                TempData["SuccessMessage"] = orchidsResponse.Message ?? "Orchids retrieved successfully";
            }
            else
            {
                TempData["ErrorMessage"] = orchidsResponse?.Message ?? "Unable to load orchids at this time.";
                _logger.LogError("Failed to load orchids. Success: {Success}, Message: {Message}", 
                    orchidsResponse?.Success, orchidsResponse?.Message);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading home page data");
            TempData["ErrorMessage"] = "An error occurred while loading the page. Please try again.";
        }

        return Page();
    }

    // API endpoint for AJAX requests
    public async Task<IActionResult> OnGetOrchidsAsync(
        string? search = null,
        string? category = null,
        bool? isNatural = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int page = 1,
        string sortBy = "Name",
        string sortDir = "Asc")
    {
        try
        {
            var orchidsResponse = await _orchidService.AdvancedSearchOrchidsAsync(
                search: search,
                categories: category,
                isNatural: isNatural,
                minPrice: minPrice,
                maxPrice: maxPrice,
                pageNumber: page,
                pageSize: 12,
                sortBy: sortBy,
                sortDirection: sortDir
            );

            if (orchidsResponse?.Success == true)
            {
                return new JsonResult(new
                {
                    success = true,
                    data = orchidsResponse.Data,
                    pagination = orchidsResponse.Pagination
                });
            }
            else
            {
                return new JsonResult(new
                {
                    success = false,
                    message = orchidsResponse?.Message ?? "Failed to load orchids"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading orchids via AJAX");
            return new JsonResult(new
            {
                success = false,
                message = "An error occurred while loading orchids"
            });
        }
    }

    // API endpoint for getting categories
    public async Task<IActionResult> OnGetCategoriesAsync()
    {
        try
        {
            var categoriesResponse = await _categoryService.GetCategoriesAsync(new CategoryQueryModel
            {
                PageSize = 100,
                SortColumn = "Name",
                SortDir = "Asc"
            });

            if (categoriesResponse?.Success == true)
            {
                return new JsonResult(new
                {
                    success = true,
                    data = categoriesResponse.Data
                });
            }
            else
            {
                return new JsonResult(new
                {
                    success = false,
                    message = categoriesResponse?.Message ?? "Failed to load categories"
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error loading categories via AJAX");
            return new JsonResult(new
            {
                success = false,
                message = "An error occurred while loading categories"
            });
        }
    }
}
