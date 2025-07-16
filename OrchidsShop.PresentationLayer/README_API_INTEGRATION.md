# OrchidsShop API Integration Guide

This document explains how to use the Presentation Layer API services to interact with the OrchidsShop.API backend. This guide has been updated to reflect all fixes and improvements made during development.

## Overview

The Presentation Layer provides a complete set of API services and models that match the structure and responses from OrchidsShop.API. These services handle HTTP communication, error handling, pagination, and data transformation.

**✅ All API integrations have been tested and verified working with real backend endpoints.**

## Architecture

```
OrchidsShop.PresentationLayer
├── Models/
│   ├── Commons/           # Common response models and pagination
│   ├── Categories/        # Category models (request/response)
│   ├── Orchids/          # Orchid models (request/response)
│   ├── Accounts/         # Account models (request/response)
│   └── Orders/           # Order models (request/response)
├── Services/
│   ├── ApiHelper.cs      # Core HTTP communication helper
│   ├── CategoryApiService.cs
│   ├── OrchidApiService.cs
│   ├── AccountApiService.cs
│   └── OrderApiService.cs
└── Examples/
    └── ApiUsageExamples.cs
```

## Setup and Configuration

### 1. Service Registration

Add these services to your `Program.cs`:

```csharp
// Register HttpClient and API services
services.AddHttpClient<ApiHelper>();
services.AddScoped<ApiHelper>();
services.AddScoped<CategoryApiService>();
services.AddScoped<OrchidApiService>();
services.AddScoped<AccountApiService>();
services.AddScoped<OrderApiService>();
```

### 2. Configuration

**⚠️ IMPORTANT:** Use HTTP, not HTTPS for local development:

```csharp
// In Constants/StringValue.cs
public const string BaseUrl = "http://localhost:5077/api/";  // ✅ Correct - HTTP
// NOT: "https://localhost:5077/api/"                        // ❌ Wrong - HTTPS
```

### 3. Backend API Setup

Ensure the backend API is running on the correct port:
- **Backend API**: `http://localhost:5077` (OrchidsShop.API)
- **Frontend**: `http://localhost:5081` (OrchidsShop.PresentationLayer)

Run both projects simultaneously:
```bash
# Terminal 1 - Backend API
cd OrchidsShop.API
dotnet run

# Terminal 2 - Frontend
cd OrchidsShop.PresentationLayer  
dotnet run
```

## API Response Structure

The OrchidsShop API uses **two different response formats** that have been properly handled:

### Categories API (Standard Format)
```csharp
// Raw API Response: {message, data, pagination}
{
  "message": "Orchid categories retrieved successfully",
  "data": [{"id": "...", "name": "Phalaenopsis"}, ...],
  "pagination": {
    "totalItemsCount": 4,
    "pageSize": 100, 
    "pageIndex": 0,
    "totalPagesCount": 1
  }
}

// Mapped to: ApiResponse<List<CategoryModel>>
ApiResponse<List<T>> {
    Message: string?,
    Data: List<T>?,        // Always a list, even for single items
    Success: bool,         // ✅ Auto-set to true for successful HTTP responses
    Pagination: PaginationModel?,
    Errors: List<string>?
}
```

### Orchids API (BLL Format)  
```csharp
// Raw API Response: {statusCode, message, isError, payload, metaData, errors}
{
  "statusCode": "200",
  "message": "Query orchids successfully",
  "isError": false,
  "payload": [{"id": "...", "name": "Beautiful Orchid"}, ...],
  "metaData": {
    "totalItemsCount": 10,
    "pageSize": 10,
    "pageIndex": 0, 
    "totalPagesCount": 1
  },
  "errors": null
}

// Mapped to: ApiResponse<List<OrchidModel>>
ApiResponse<List<T>> {
    Success: bool,         // Mapped from !isError
    Message: string?,      // From message
    Data: List<T>?,        // From payload
    Pagination: PaginationModel?, // From metaData
    Errors: List<string>?  // From errors
}
```

### POST/PUT/DELETE Operations (Endpoints)
```csharp
ApiOperationResponse {
    Message: string?,
    Success: bool,
    Errors: List<string>?
}
```

### Pagination Metadata (Fixed Property Mapping)
```csharp
PaginationModel {
    // JSON property mapping for API compatibility
    [JsonPropertyName("pageIndex")]
    PageIndex: int,
    
    [JsonPropertyName("pageSize")] 
    PageSize: int,
    
    [JsonPropertyName("totalItemsCount")]
    TotalItemsCount: int,
    
    [JsonPropertyName("totalPagesCount")]
    TotalPagesCount: int,
    
    // Computed properties for consistent interface
    PageNumber: int => PageIndex + 1,
    TotalRecords: int => TotalItemsCount, 
    TotalPages: int => TotalPagesCount
}
```

## Usage Examples

### Category Operations

```csharp
public class CategoryController : Controller
{
    private readonly CategoryApiService _categoryService;
    
    public CategoryController(CategoryApiService categoryService)
    {
        _categoryService = categoryService;
    }
    
    // Get all categories for dropdown
    public async Task<IActionResult> GetCategories()
    {
        var response = await _categoryService.GetAllCategoriesForDropdownAsync();
        
        if (response?.Success == true && response.Data != null)
        {
            return Json(response.Data);
        }
        
        return BadRequest(response?.Message ?? "Failed to get categories");
    }
    
    // Search categories with pagination
    public async Task<IActionResult> SearchCategories(string search, int page = 1)
    {
        var response = await _categoryService.SearchCategoriesAsync(search, page, 10);
        
        return Json(new {
            success = response?.Success ?? false,
            data = response?.Data ?? new List<CategoryModel>(),
            pagination = response?.Pagination,
            message = response?.Message
        });
    }
    
    // Create new category
    [HttpPost]
    public async Task<IActionResult> CreateCategory([FromBody] CategoryRequestModel model)
    {
        var response = await _categoryService.CreateCategoryAsync(model);
        
        if (response?.Success == true)
        {
            return Ok(new { message = response.Message });
        }
        
        return BadRequest(new { 
            message = response?.Message,
            errors = response?.Errors 
        });
    }
}
```

### Orchid Operations

```csharp
// Get orchids for product catalog
public async Task<IActionResult> GetOrchids(
    string? search = null,
    string? category = null,
    bool? isNatural = null,
    decimal? minPrice = null,
    decimal? maxPrice = null,
    int page = 1,
    int pageSize = 12)
{
    var response = await _orchidService.AdvancedSearchOrchidsAsync(
        search, category, isNatural, minPrice, maxPrice,
        page, pageSize, "Name", "Asc");
    
    return Json(new {
        orchids = response?.Data ?? new List<OrchidModel>(),
        pagination = response?.Pagination,
        success = response?.Success ?? false
    });
}

// Get single orchid details
public async Task<IActionResult> GetOrchidDetails(Guid id)
{
    var response = await _orchidService.GetOrchidByIdAsync(id);
    
    if (response?.Success == true && response.Data?.Any() == true)
    {
        return Json(response.Data.First());
    }
    
    return NotFound("Orchid not found");
}

// Create new orchid
[HttpPost]
public async Task<IActionResult> CreateOrchid([FromBody] OrchidRequestModel model)
{
    var response = await _orchidService.CreateOrchidAsync(model);
    
    return Json(new {
        success = response?.Success ?? false,
        message = response?.Message,
        errors = response?.Errors
    });
}
```

### Account Operations

```csharp
// User login
[HttpPost]
public async Task<IActionResult> Login([FromBody] LoginRequestModel model)
{
    var response = await _accountService.LoginAsync(model);
    
    if (response?.Success == true)
    {
        // Set session or create JWT token
        HttpContext.Session.SetString("UserEmail", model.Email);
        return Ok(new { message = "Login successful" });
    }
    
    return BadRequest(new { 
        message = response?.Message ?? "Login failed",
        errors = response?.Errors 
    });
}

// User registration
[HttpPost]
public async Task<IActionResult> Register([FromBody] RegisterRequestModel model)
{
    var response = await _accountService.RegisterAsync(model);
    
    return Json(new {
        success = response?.Success ?? false,
        message = response?.Message,
        errors = response?.Errors
    });
}

// Get user profile
public async Task<IActionResult> GetProfile(Guid userId)
{
    var response = await _accountService.GetAccountByIdAsync(userId);
    
    if (response?.Success == true && response.Data?.Any() == true)
    {
        return Json(response.Data.First());
    }
    
    return NotFound("User not found");
}
```

### Order Operations

```csharp
// Create order from shopping cart
[HttpPost]
public async Task<IActionResult> CreateOrder([FromBody] CreateOrderViewModel model)
{
    var orderDetails = model.CartItems.Select(item => new OrderDetailRequestModel
    {
        OrchidId = item.OrchidId,
        Quantity = item.Quantity
    }).ToList();
    
    var response = await _orderService.CreateOrderFromCartAsync(model.CustomerId, orderDetails);
    
    return Json(new {
        success = response?.Success ?? false,
        message = response?.Message,
        errors = response?.Errors
    });
}

// Get customer order history
public async Task<IActionResult> GetOrderHistory(Guid customerId, int page = 1)
{
    var response = await _orderService.GetCustomerOrderHistoryAsync(customerId, page, 10);
    
    return Json(new {
        orders = response?.Data ?? new List<OrderModel>(),
        pagination = response?.Pagination,
        success = response?.Success ?? false
    });
}

// Update order status (admin)
[HttpPost]
public async Task<IActionResult> UpdateOrderStatus(Guid orderId, string status)
{
    var response = await _orderService.UpdateOrderStatusAsync(orderId, status);
    
    return Json(new {
        success = response?.Success ?? false,
        message = response?.Message
    });
}
```

## Error Handling Best Practices

### 1. Always Check Response
```csharp
var response = await _orchidService.GetOrchidsAsync();

// Check for null response
if (response == null)
{
    // Handle network/connection error
    _logger.LogError("API response was null - possible network issue");
    return StatusCode(500, "Connection error");
}

// Check success flag (automatically set for successful HTTP responses)
if (!response.Success)
{
    // Handle API error
    _logger.LogWarning("API returned error. Message: {Message}, Errors: {Errors}", 
        response.Message, string.Join(", ", response.Errors ?? new List<string>()));
    return BadRequest(new { 
        message = response.Message,
        errors = response.Errors 
    });
}

// Check data availability
if (response.Data == null || !response.Data.Any())
{
    // Handle empty result (this is normal, not an error)
    _logger.LogInformation("No data returned from API - empty result set");
    return Ok(new { message = "No data found", data = new List<OrchidModel>() });
}

// Process successful response
_logger.LogInformation("Successfully retrieved {Count} items from API", response.Data.Count);
return Ok(response.Data);
```

### 2. Exception Handling
```csharp
try
{
    var response = await _categoryService.GetCategoriesAsync();
    // Process response...
}
catch (HttpRequestException ex)
{
    // Handle HTTP-specific errors
    _logger.LogError(ex, "HTTP error while calling API");
    return StatusCode(503, "Service unavailable");
}
catch (TaskCanceledException ex)
{
    // Handle timeout
    _logger.LogError(ex, "API request timeout");
    return StatusCode(408, "Request timeout");
}
catch (Exception ex)
{
    // Handle general errors
    _logger.LogError(ex, "Unexpected error");
    return StatusCode(500, "Internal server error");
}
```

## Frontend Integration

### JavaScript/jQuery Example

```javascript
// Get orchids with filtering
async function loadOrchids(page = 1, filters = {}) {
    try {
        const params = new URLSearchParams({
            page: page,
            pageSize: 12,
            ...filters
        });
        
        const response = await fetch(`/Orchids/GetOrchids?${params}`);
        const result = await response.json();
        
        if (result.success) {
            displayOrchids(result.orchids);
            updatePagination(result.pagination);
        } else {
            showError(result.message || 'Failed to load orchids');
        }
    } catch (error) {
        showError('Network error: ' + error.message);
    }
}

// Create new orchid
async function createOrchid(orchidData) {
    try {
        const response = await fetch('/Orchids/CreateOrchid', {
            method: 'POST',
            headers: {
                'Content-Type': 'application/json',
            },
            body: JSON.stringify(orchidData)
        });
        
        const result = await response.json();
        
        if (result.success) {
            showSuccess(result.message || 'Orchid created successfully');
            loadOrchids(); // Refresh list
        } else {
            showError(result.message || 'Failed to create orchid');
            if (result.errors) {
                result.errors.forEach(error => showError(error));
            }
        }
    } catch (error) {
        showError('Network error: ' + error.message);
    }
}
```

## Testing the Integration

### 1. Test API Connectivity
```csharp
[HttpGet]
public async Task<IActionResult> TestConnection()
{
    try
    {
        // Test both API formats
        var categoryResponse = await _categoryService.GetCategoriesAsync();
        var orchidResponse = await _orchidService.GetOrchidsAsync();
        
        return Json(new { 
            categoriesConnected = categoryResponse?.Success == true,
            orchidsConnected = orchidResponse?.Success == true,
            categoryMessage = categoryResponse?.Message,
            orchidMessage = orchidResponse?.Message,
            totalCategories = categoryResponse?.Data?.Count ?? 0,
            totalOrchids = orchidResponse?.Data?.Count ?? 0
        });
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "API connectivity test failed");
        return Json(new { 
            connected = false,
            message = ex.Message 
        });
    }
}
```

### 2. Known Issues & Solutions

#### Issue: Categories Not Loading (RESOLVED ✅)
- **Problem**: Categories API doesn't include `Success` field in response
- **Solution**: `ApiHelper.GetAsync()` now automatically sets `Success = true` for successful HTTP responses

#### Issue: Pagination Property Mismatch (RESOLVED ✅)  
- **Problem**: API returns `{totalItemsCount, pageIndex, totalPagesCount}` but models expected `{TotalRecords, PageNumber, TotalPages}`
- **Solution**: Added JSON property name attributes to `PaginationModel` with computed properties for compatibility

#### Issue: HTTP vs HTTPS (RESOLVED ✅)
- **Problem**: Frontend using HTTPS, backend using HTTP causing connection failures
- **Solution**: Updated `StringValue.BaseUrl` to use HTTP (`http://localhost:5077/api/`)

#### Issue: Order Sorting Property Mismatch (RESOLVED ✅)
- **Problem**: Frontend sends `TotalAmount` but backend entity has `TotalAmound` (typo)
- **Solution**: Property mapping handled in service layer to map `TotalAmount` → `TotalAmound`

#### Issue: Property Name Typo (KNOWN LIMITATION ⚠️)
- **Problem**: Backend API uses `IsNarutal` (with typo) instead of `IsNatural`  
- **Solution**: Frontend models maintain the typo to match API exactly

### 3. Validate Response Structure
The API services handle both response formats automatically:

- **Categories API**: `{ message, data: [], pagination }` → `ApiResponse<List<T>>`  
- **Orchids API**: `{ statusCode, message, isError, payload, metaData, errors }` → `ApiResponse<List<T>>`
- **Endpoints** (POST/PUT/DELETE): `{ message, success, errors?: [] }` → `ApiOperationResponse`

### 4. Check Pagination
All GET operations support pagination with automatic property mapping:
```csharp
var response = await _orchidService.GetOrchidsAsync(new OrchidQueryModel 
{
    PageNumber = 1,
    PageSize = 10,
    SortColumn = "Name",
    SortDir = "Asc"
});

// Access pagination metadata (works for both API formats)
var totalPages = response?.Pagination?.TotalPages ?? 1;        // Computed property
var totalRecords = response?.Pagination?.TotalRecords ?? 0;    // Computed property
var pageNumber = response?.Pagination?.PageNumber ?? 1;        // PageIndex + 1
var pageSize = response?.Pagination?.PageSize ?? 10;           // Direct mapping
```

## API Endpoints Reference

### Categories
- `GET /api/orchid-categories` - Get categories with pagination/filtering
- `POST /api/orchid-categories` - Create categories (bulk)
- `PUT /api/orchid-categories/{id}` - Update category
- `PATCH /api/orchid-categories/{id}` - Partial update category
- `DELETE /api/orchid-categories/{id}` - Delete category

### Orchids
- `GET /api/orchids` - Get orchids with advanced filtering
- `POST /api/orchids` - Create orchid
- `PUT /api/orchids` - Update orchid
- `DELETE /api/orchids/{id}` - Delete orchid

### Accounts
- `POST /api/accounts/login` - User login
- `POST /api/accounts/register` - User registration
- `POST /api/accounts` - Create account (admin)
- `GET /api/accounts/{id}` - Get account by ID
- `PUT /api/accounts/{id}` - Update account
- `GET /api/accounts/roles` - Get available roles

### Orders
- `GET /api/orders` - Get orders with filtering
- `POST /api/orders` - Create order
- `PUT /api/orders` - Update order
- `DELETE /api/orders` - Delete order

For detailed API documentation and testing, refer to `OrchidsShop.API.http` file.

## Recent Updates & Fixes

### ✅ Fixed Issues (Latest Update)

1. **Categories Loading Issue** - Resolved JSON deserialization problems
2. **Pagination Property Mapping** - Fixed property name mismatches  
3. **HTTP/HTTPS Configuration** - Corrected URL scheme for local development
4. **Response Format Handling** - Added support for dual API response formats
5. **Auto-Success Detection** - Automatic success flag setting for Categories API
6. **Order Sorting Issue** - Fixed TotalAmount property mapping for order queries
7. **UI Improvements** - Removed role display from customer information and hidden print/download actions

### 🔧 ApiHelper Improvements

The `ApiHelper` class now includes:
- **Dual Format Support**: `GetAsync()` for Categories, `GetBllAsync()` for Orchids
- **Auto-Success Setting**: Automatically sets `Success = true` for successful HTTP responses
- **Property Mapping**: Proper JSON property name handling with `PropertyNamingPolicy = null`
- **Error Handling**: Comprehensive exception handling with detailed logging

### 📱 Frontend Integration Status

**✅ Fully Working Features:**
- Categories dropdown population
- Orchid grid display with pagination
- Search and filtering functionality  
- Responsive design across all devices
- Real-time API connectivity
- Error message display
- Loading states and feedback
- Order management with status tracking
- Customer order history with filtering and sorting

**🎨 Recent UI Improvements:**
- Removed role display from customer information for privacy
- Hidden print and download actions (reserved for future implementation)
- Enhanced order details page with timeline visualization
- Improved order status badges and action buttons

### 🚀 Usage in Production

The current implementation is production-ready for local development. For deployment:

1. **Update BaseUrl** in `StringValue.cs` to point to production API
2. **Configure HTTPS** certificates for production environment  
3. **Environment Variables** for different deployment stages
4. **Logging Configuration** for production monitoring

```csharp
// Example production configuration
public static class StringValue 
{
    public static string BaseUrl => Environment.GetEnvironmentVariable("API_BASE_URL") 
                                   ?? "https://api.orchidshop.com/api/";
}
```

### 📋 Future Considerations

- Consider implementing retry logic for failed API calls
- Add response caching for frequently accessed data
- Implement request timeout configuration
- Add API health check endpoints
- Consider using HttpClientFactory for better connection management 