# OrchidsShop API Integration Guide

This document explains how to use the Presentation Layer API services to interact with the OrchidsShop.API backend.

## Overview

The Presentation Layer provides a complete set of API services and models that match the structure and responses from OrchidsShop.API. These services handle HTTP communication, error handling, pagination, and data transformation.

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

Update your base URL in `Constants/StringValue.cs`:

```csharp
public const string BaseUrl = "https://localhost:5077/api/";
```

## API Response Structure

All API services return consistent response structures:

### GET Operations (Controllers)
```csharp
ApiResponse<List<T>> {
    Message: string?,
    Data: List<T>?,        // Always a list, even for single items
    Success: bool,
    Pagination: PaginationModel?,
    Errors: List<string>?
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

### Pagination Metadata
```csharp
PaginationModel {
    PageNumber: int,
    PageSize: int,
    TotalRecords: int,
    TotalPages: int
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
    return StatusCode(500, "Connection error");
}

// Check success flag
if (!response.Success)
{
    // Handle API error
    return BadRequest(new { 
        message = response.Message,
        errors = response.Errors 
    });
}

// Check data availability
if (response.Data == null || !response.Data.Any())
{
    // Handle empty result
    return Ok(new { message = "No data found", data = new List<OrchidModel>() });
}

// Process successful response
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
        var response = await _categoryService.GetCategoriesAsync();
        return Json(new { 
            connected = response != null,
            message = response?.Success == true ? "API connected successfully" : response?.Message
        });
    }
    catch (Exception ex)
    {
        return Json(new { 
            connected = false,
            message = ex.Message 
        });
    }
}
```

### 2. Validate Response Structure
The API services are designed to match the exact response structure from OrchidsShop.API:

- **Controllers** (GET operations) return `{ message, data: [], pagination }`
- **Endpoints** (POST/PUT/DELETE) return `{ message, success, errors?: [] }`

### 3. Check Pagination
All GET operations support pagination:
```csharp
var response = await _orchidService.GetOrchidsAsync(new OrchidQueryModel 
{
    PageNumber = 1,
    PageSize = 10,
    SortColumn = "Name",
    SortDir = "Asc"
});

// Access pagination metadata
var totalPages = response?.Pagination?.TotalPages ?? 1;
var totalRecords = response?.Pagination?.TotalRecords ?? 0;
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