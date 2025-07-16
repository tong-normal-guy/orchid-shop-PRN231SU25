using OrchidsShop.PresentationLayer.Models.Accounts;
using OrchidsShop.PresentationLayer.Models.Categories;
using OrchidsShop.PresentationLayer.Models.Commons;
using OrchidsShop.PresentationLayer.Models.Orchids;
using OrchidsShop.PresentationLayer.Models.Orders;
using OrchidsShop.PresentationLayer.Services;

namespace OrchidsShop.PresentationLayer.Examples;

/// <summary>
/// Examples demonstrating how to use the API services to interact with OrchidsShop.API
/// This class shows proper usage patterns and response handling
/// </summary>
public class ApiUsageExamples
{
    private readonly CategoryApiService _categoryService;
    private readonly OrchidApiService _orchidService;
    private readonly AccountApiService _accountService;
    private readonly OrderApiService _orderService;

    public ApiUsageExamples(
        CategoryApiService categoryService,
        OrchidApiService orchidService,
        AccountApiService accountService,
        OrderApiService orderService)
    {
        _categoryService = categoryService;
        _orchidService = orchidService;
        _accountService = accountService;
        _orderService = orderService;
    }

    #region Category Examples

    /// <summary>
    /// Example: Get all categories for a dropdown menu
    /// </summary>
    public async Task<List<CategoryModel>> GetCategoriesForDropdownExample()
    {
        var response = await _categoryService.GetAllCategoriesForDropdownAsync();
        
        if (response?.Success == true && response.Data != null)
        {
            return response.Data;
        }
        
        // Handle error case
        Console.WriteLine($"Error getting categories: {response?.Message}");
        return new List<CategoryModel>();
    }

    /// <summary>
    /// Example: Search categories with pagination
    /// </summary>
    public async Task<(List<CategoryModel> categories, PaginationModel? pagination)> SearchCategoriesExample(string searchTerm)
    {
        var response = await _categoryService.SearchCategoriesAsync(searchTerm, pageNumber: 1, pageSize: 10);
        
        if (response?.Success == true && response.Data != null)
        {
            return (response.Data, response.Pagination);
        }
        
        return (new List<CategoryModel>(), null);
    }

    /// <summary>
    /// Example: Create a new category
    /// </summary>
    public async Task<bool> CreateCategoryExample(string categoryName)
    {
        var categoryRequest = new CategoryRequestModel
        {
            Name = categoryName
        };

        var response = await _categoryService.CreateCategoryAsync(categoryRequest);
        
        if (response?.Success == true)
        {
            Console.WriteLine($"Category created successfully: {response.Message}");
            return true;
        }
        
        Console.WriteLine($"Failed to create category: {response?.Message}");
        if (response?.Errors != null)
        {
            foreach (var error in response.Errors)
            {
                Console.WriteLine($"Error: {error}");
            }
        }
        return false;
    }

    #endregion

    #region Orchid Examples

    /// <summary>
    /// Example: Get orchids for product catalog with filtering
    /// </summary>
    public async Task<(List<OrchidModel> orchids, PaginationModel? pagination)> GetOrchidCatalogExample(
        string? search = null, 
        string? category = null, 
        bool? isNatural = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int page = 1)
    {
        var response = await _orchidService.AdvancedSearchOrchidsAsync(
            search: search,
            categories: category,
            isNatural: isNatural,
            minPrice: minPrice,
            maxPrice: maxPrice,
            pageNumber: page,
            pageSize: 12,
            sortBy: "Name",
            sortDirection: "Asc"
        );

        if (response?.Success == true && response.Data != null)
        {
            return (response.Data, response.Pagination);
        }

        return (new List<OrchidModel>(), null);
    }

    /// <summary>
    /// Example: Get single orchid details
    /// </summary>
    public async Task<OrchidModel?> GetOrchidDetailsExample(Guid orchidId)
    {
        var response = await _orchidService.GetOrchidByIdAsync(orchidId);
        
        if (response?.Success == true && response.Data?.Any() == true)
        {
            return response.Data.First();
        }
        
        return null;
    }

    /// <summary>
    /// Example: Create a new orchid
    /// </summary>
    public async Task<bool> CreateOrchidExample()
    {
        var orchidRequest = new OrchidRequestModel
        {
            Name = "Beautiful White Phalaenopsis",
            Description = "A stunning white orchid perfect for home decoration",
            Url = "https://example.com/images/white-orchid.jpg",
            Price = 45.99m,
            IsNatural = true,
            CategoryId = Guid.Parse("550e8400-e29b-41d4-a716-446655440000") // Replace with actual category ID
        };

        var response = await _orchidService.CreateOrchidAsync(orchidRequest);
        return response?.Success == true;
    }

    #endregion

    #region Account Examples

    /// <summary>
    /// Example: User login
    /// </summary>
    public async Task<bool> LoginExample(string email, string password)
    {
        var loginRequest = new LoginRequestModel
        {
            Email = email,
            Password = password
        };

        var response = await _accountService.LoginAsync(loginRequest);
        
        if (response?.Success == true)
        {
            Console.WriteLine("Login successful!");
            // Here you would typically store user session/token
            return true;
        }
        
        Console.WriteLine($"Login failed: {response?.Message}");
        return false;
    }

    /// <summary>
    /// Example: User registration
    /// </summary>
    public async Task<bool> RegisterExample(string email, string name, string password)
    {
        var registerRequest = new RegisterRequestModel
        {
            Email = email,
            Name = name,
            Password = password,
            ConfirmPassword = password,
            Role = "Customer"
        };

        var response = await _accountService.RegisterAsync(registerRequest);
        return response?.Success == true;
    }

    /// <summary>
    /// Example: Get user profile
    /// </summary>
    public async Task<AccountModel?> GetUserProfileExample(Guid userId)
    {
        var response = await _accountService.GetAccountByIdAsync(userId);
        
        if (response?.Success == true && response.Data?.Any() == true)
        {
            return response.Data.First();
        }
        
        return null;
    }

    #endregion

    #region Order Examples

    /// <summary>
    /// Example: Create a simple order with one item
    /// </summary>
    public async Task<bool> CreateSimpleOrderExample(Guid customerId, Guid orchidId, int quantity)
    {
        var response = await _orderService.CreateSimpleOrderAsync(customerId, orchidId, quantity);
        return response?.Success == true;
    }

    /// <summary>
    /// Example: Create order from shopping cart
    /// </summary>
    public async Task<bool> CreateOrderFromCartExample(Guid customerId, List<(Guid orchidId, int quantity)> cartItems)
    {
        var orderDetails = cartItems.Select(item => new OrderDetailRequestModel
        {
            OrchidId = item.orchidId,
            Quantity = item.quantity
        }).ToList();

        var response = await _orderService.CreateOrderFromCartAsync(customerId, orderDetails);
        return response?.Success == true;
    }

    /// <summary>
    /// Example: Get customer order history
    /// </summary>
    public async Task<(List<OrderModel> orders, PaginationModel? pagination)> GetCustomerOrderHistoryExample(Guid customerId, int page = 1)
    {
        var response = await _orderService.GetCustomerOrderHistoryAsync(customerId, page, pageSize: 10);
        
        if (response?.Success == true && response.Data != null)
        {
            return (response.Data, response.Pagination);
        }
        
        return (new List<OrderModel>(), null);
    }

    /// <summary>
    /// Example: Update order status (for admin/staff)
    /// </summary>
    public async Task<bool> UpdateOrderStatusExample(Guid orderId, string newStatus)
    {
        var response = await _orderService.UpdateOrderStatusAsync(orderId, newStatus);
        return response?.Success == true;
    }

    #endregion

    #region Error Handling Examples

    /// <summary>
    /// Example: Comprehensive error handling pattern
    /// </summary>
    public async Task<string> HandleApiResponseExample()
    {
        try
        {
            var response = await _categoryService.GetCategoriesAsync();
            
            if (response == null)
            {
                return "No response received from API";
            }
            
            if (!response.Success)
            {
                var errorMessage = response.Message ?? "Unknown error occurred";
                
                if (response.Errors != null && response.Errors.Any())
                {
                    errorMessage += $" Details: {string.Join(", ", response.Errors)}";
                }
                
                return $"API Error: {errorMessage}";
            }
            
            if (response.Data == null || !response.Data.Any())
            {
                return "No data returned from API";
            }
            
            return $"Success: Retrieved {response.Data.Count} categories";
        }
        catch (Exception ex)
        {
            return $"Exception occurred: {ex.Message}";
        }
    }

    #endregion

    #region Pagination Examples

    /// <summary>
    /// Example: Handle paginated results
    /// </summary>
    public async Task<(List<OrchidModel> currentPage, int totalPages, int totalItems)> GetPaginatedOrchidsExample(int page, int pageSize)
    {
        var response = await _orchidService.GetOrchidsForCatalogAsync(page, pageSize);
        
        if (response?.Success == true && response.Data != null)
        {
            var totalPages = response.Pagination?.TotalPages ?? 1;
            var totalItems = response.Pagination?.TotalRecords ?? response.Data.Count;
            
            return (response.Data, totalPages, totalItems);
        }
        
        return (new List<OrchidModel>(), 1, 0);
    }

    #endregion
}

/// <summary>
/// Example of how to register these services in Program.cs
/// </summary>
public static class ServiceRegistrationExample
{
    public static void ConfigureApiServices(IServiceCollection services)
    {
        // Register HttpClient
        services.AddHttpClient<ApiHelper>();
        
        // Register API services
        services.AddScoped<ApiHelper>();
        services.AddScoped<CategoryApiService>();
        services.AddScoped<OrchidApiService>();
        services.AddScoped<AccountApiService>();
        services.AddScoped<OrderApiService>();
        
        // Register the example class for demonstration
        services.AddScoped<ApiUsageExamples>();
    }
} 