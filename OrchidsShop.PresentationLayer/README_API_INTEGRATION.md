# OrchidsShop API Integration Guide

This document explains how to use the Presentation Layer API services to interact with the OrchidsShop.API backend. This guide has been updated to reflect all fixes and improvements made during development, including the new admin functionality.

## Overview

The Presentation Layer provides a complete set of API services and models that match the structure and responses from OrchidsShop.API. These services handle HTTP communication, error handling, pagination, and data transformation.

**✅ All API integrations have been tested and verified working with real backend endpoints.**

**🆕 NEW: Admin functionality with full CRUD operations for orchids, orders, and accounts management.**

**🔧 FIXED: Account page JSON deserialization issue resolved.**

## 🎥 Demo Video
**Watch the API integration in action:** [OrchidsShop Demo](https://youtu.be/-Gb_q9g5eVs)

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
├── Pages/
│   ├── Admin/            # 🆕 Admin management pages
│   │   ├── Orchids.cshtml          # Admin dashboard with orchid list
│   │   ├── Orders.cshtml           # Order management with status updates
│   │   ├── Accounts.cshtml         # Account management
│   │   ├── CreateOrchid.cshtml     # Create new orchid form
│   │   └── EditOrchid.cshtml       # Edit existing orchid form
│   └── Examples/
│       └── ApiUsageExamples.cs
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

## 🆕 Admin Functionality

### Admin Access Control

The admin functionality is protected by session-based authentication:

```csharp
// Check admin privileges in page models
var userRole = HttpContext.Session.GetString("UserRole");
if (string.IsNullOrEmpty(userRole) || userRole != "ADMIN")
{
    TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
    return RedirectToPage("/Auth/Login");
}
```

### Admin Pages

1. **Admin Dashboard** (`/Admin/Orchids`)
   - View all orchids with search and filtering
   - Statistics overview (total orchids, categories, orders, customers)
   - Quick actions for each orchid (edit, view details)

2. **Order Management** (`/Admin/Orders`)
   - View all customer orders with filtering
   - Update order status (Pending, Processing, Shipped, Delivered, Cancelled)
   - Order statistics and management tools
   - Real-time status updates via AJAX

3. **Account Management** (`/Admin/Accounts`)
   - View and manage user accounts
   - Role-based access control
   - User statistics and management

4. **Create Orchid** (`/Admin/CreateOrchid`)
   - Form to create new orchids
   - Real-time preview and validation
   - Category selection dropdown

5. **Edit Orchid** (`/Admin/EditOrchid/{id}`)
   - Pre-populated form for editing existing orchids
   - Form validation and error handling
   - Preview of current orchid information

### Admin Features

- **Responsive Design**: Works on desktop, tablet, and mobile
- **Real-time Validation**: Client-side and server-side validation
- **Search & Filter**: Filter orchids by name and category
- **Statistics Dashboard**: Overview of system metrics
- **User-friendly Interface**: Modern UI with loading states and notifications
- **Order Status Management**: Update order status with AJAX calls
- **Account Management**: View and manage user accounts

## 🔧 Recent API Fixes

### Account Page JSON Deserialization Issue

**Problem**: The account page was failing with JSON deserialization errors when trying to view user profiles.

**Root Cause**: The API was returning `OperationResult<List<QueryAccountResponse>>` objects, but the frontend was trying to deserialize them as direct arrays.

**Solution**: 
1. **Updated AccountService** to return lists for all read operations (following OrchidShop pattern)
2. **Fixed ApiHelper.GetArrayAsync()** to handle BLL Operation Result format
3. **Added proper error handling** and fallback mechanisms

**Code Changes**:
```csharp
// New method in AccountService
public async Task<OperationResult<List<QueryAccountResponse>>> GetCurrentProfileAsync(Guid id)
{
    // Returns account data wrapped in a list
    var response = _mapper.Map<QueryAccountResponse>(account);
    var accountList = new List<QueryAccountResponse> { response };
    return OperationResult<List<QueryAccountResponse>>.Success(accountList, StatusCode.Ok, "Account retrieved successfully.");
}

// Fixed ApiHelper.GetArrayAsync method
public async Task<ApiResponse<List<TData>>?> GetArrayAsync<TData>(string url)
{
    // Try BLL format first, then fallback to direct array
    var bllResponse = JsonSerializer.Deserialize<BllOperationResponse<List<TData>>>(responseData, _jsonOptions);
    if (bllResponse != null)
    {
        return new ApiResponse<List<TData>>
        {
            Success = !bllResponse.IsError,
            Message = bllResponse.Message,
            Data = bllResponse.Payload,
            // ... handle pagination and errors
        };
    }
    // Fallback to direct array format
}
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

### 🆕 Admin Orchid Management

```csharp
// Admin page model example
public class OrchidsModel : PageModel
{
    private readonly OrchidApiService _orchidService;
    private readonly CategoryApiService _categoryService;
    
    public List<OrchidModel> Orchids { get; set; } = new();
    public List<CategoryModel> Categories { get; set; } = new();
    
    public async Task<IActionResult> OnGetAsync()
    {
        // Check admin privileges
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || userRole != "ADMIN")
        {
            TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
            return RedirectToPage("/Auth/Login");
        }
        
        // Load data in parallel
        var tasks = new[]
        {
            LoadOrchidsAsync(),
            LoadCategoriesAsync()
        };
        
        await Task.WhenAll(tasks);
        return Page();
    }
    
    private async Task LoadOrchidsAsync()
    {
        var response = await _orchidService.GetOrchidsAsync(new OrchidQueryModel
        {
            PageSize = 100, // Get all orchids for admin
            SortColumn = "Name",
            SortDir = "Asc"
        });

        if (response?.Success == true && response.Data != null)
        {
            Orchids = response.Data;
        }
    }
}
```

### 🆕 Admin Order Management

```csharp
// Admin order management example
public class OrdersModel : PageModel
{
    private readonly OrderApiService _orderService;
    
    public List<OrderModel> Orders { get; set; } = new();
    
    public async Task<IActionResult> OnGetAsync()
    {
        // Check admin privileges
        var userRole = HttpContext.Session.GetString("UserRole");
        if (string.IsNullOrEmpty(userRole) || userRole != "ADMIN")
        {
            TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
            return RedirectToPage("/Auth/Login");
        }
        
        await LoadOrdersAsync();
        return Page();
    }
    
    private async Task LoadOrdersAsync()
    {
        var response = await _orderService.GetOrdersForAdminAsync(new OrderQueryModel
        {
            PageSize = 100, // Get all orders for admin
            SortColumn = "OrderDate",
            SortDir = "Desc",
            IsManagment = true // Set to true for admin management
        });

        if (response?.Success == true && response.Data != null)
        {
            Orders = response.Data;
        }
    }
    
    // Handle order status updates
    public async Task<IActionResult> OnPostUpdateStatusAsync()
    {
        // Implementation for updating order status
        // Returns JSON response for AJAX calls
    }
}
```

### 🔧 Fixed Account Profile Management

```csharp
// Account profile management (now working correctly)
public class AccountPageModel : PageModel
{
    private readonly AccountApiService _accountApiService;
    
    public AccountModel? Account { get; set; }
    public bool IsLoaded { get; set; } = false;
    
    public async Task OnGetAsync()
    {
        var jwtToken = HttpContext.Session.GetString("JwtToken");
        if (string.IsNullOrEmpty(jwtToken))
        {
            ErrorMessage = "Please log in to view your account details.";
            return;
        }
        
        var response = await _accountApiService.GetCurrentProfileAsync();
        
        if (response?.Success == true && response.Data != null && response.Data.Count > 0)
        {
            Account = response.Data.FirstOrDefault();
            IsLoaded = true;
        }
        else
        {
            ErrorMessage = response?.Message ?? "Failed to load account details.";
        }
    }
}
```

### Category Management

```csharp
// Get categories with pagination
var response = await _categoryService.GetCategoriesAsync(new CategoryQueryModel
{
    PageNumber = 1,
    PageSize = 10,
    Search = "Phalaenopsis"
});

if (response?.Success == true && response.Data != null)
{
    var categories = response.Data;
    var totalPages = response.Pagination?.TotalPages ?? 1;
}
```

### Orchid Management

```csharp
// Get orchids with advanced filtering
var response = await _orchidService.GetOrchidsAsync(new OrchidQueryModel
{
    PageNumber = 1,
    PageSize = 20,
    Search = "Beautiful",
    CategoryId = categoryId,
    MinPrice = 10.0m,
    MaxPrice = 100.0m,
    IsNatural = true,
    SortColumn = "Price",
    SortDir = "Asc"
});

if (response?.Success == true && response.Data != null)
{
    var orchids = response.Data;
    var totalRecords = response.Pagination?.TotalRecords ?? 0;
}
```

### Order Management

```csharp
// Get orders for customer
var response = await _orderService.GetOrdersAsync(new OrderQueryModel
{
    PageNumber = 1,
    PageSize = 10,
    AccountId = currentUserId,
    SortColumn = "OrderDate",
    SortDir = "Desc"
});

if (response?.Success == true && response.Data != null)
{
    var orders = response.Data;
}
```

### Account Management

```csharp
// Login user
var loginResponse = await _accountService.LoginAsync(new LoginRequestModel
{
    Email = "user@example.com",
    Password = "password123"
});

if (loginResponse?.Success == true && loginResponse.Data != null)
{
    // Store JWT token in session
    HttpContext.Session.SetString("JwtToken", loginResponse.Data.Token);
    HttpContext.Session.SetString("UserEmail", loginResponse.Data.Email);
    HttpContext.Session.SetString("UserRole", loginResponse.Data.Role);
}
```

## Error Handling

### Comprehensive Error Handling

All API services include comprehensive error handling:

```csharp
try
{
    var response = await _service.GetDataAsync();
    
    if (response?.Success == true && response.Data != null)
    {
        // Handle success
        return response.Data;
    }
    else
    {
        // Handle API errors
        _logger.LogWarning("API returned error: {Message}", response?.Message);
        return new List<T>();
    }
}
catch (Exception ex)
{
    // Handle network/deserialization errors
    _logger.LogError(ex, "Error calling API");
    return new List<T>();
}
```

### HTTP Status Code Handling

The `ApiHelper` automatically handles different HTTP status codes:

```csharp
if (!response.IsSuccessStatusCode)
{
    var errorMessage = await response.Content.ReadAsStringAsync();
    return new ApiResponse<List<TData>>
    {
        Success = false,
        Message = errorMessage,
        Errors = new List<string> { errorMessage }
    };
}
```

## Pagination Support

### Automatic Pagination Handling

All list endpoints support pagination with automatic metadata mapping:

```csharp
// Request with pagination
var queryModel = new OrchidQueryModel
{
    PageNumber = 2,
    PageSize = 20,
    SortColumn = "Name",
    SortDir = "Asc"
};

var response = await _orchidService.GetOrchidsAsync(queryModel);

// Access pagination metadata
if (response?.Pagination != null)
{
    var currentPage = response.Pagination.PageNumber;
    var totalPages = response.Pagination.TotalPages;
    var totalRecords = response.Pagination.TotalRecords;
    var hasNextPage = currentPage < totalPages;
    var hasPreviousPage = currentPage > 1;
}
```

## Authentication

### JWT Token Management

The API services automatically handle JWT token authentication:

```csharp
private void AddAuthorizationHeader()
{
    var jwtToken = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
    if (!string.IsNullOrEmpty(jwtToken))
    {
        _httpClient.DefaultRequestHeaders.Authorization = 
            new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
    }
}
```

### Session Management

User sessions are managed through ASP.NET Core sessions:

```csharp
// Store user data in session after login
HttpContext.Session.SetString("JwtToken", loginResponse.Data.Token);
HttpContext.Session.SetString("UserEmail", loginResponse.Data.Email);
HttpContext.Session.SetString("UserRole", loginResponse.Data.Role);

// Retrieve user data from session
var userEmail = HttpContext.Session.GetString("UserEmail");
var userRole = HttpContext.Session.GetString("UserRole");
```

## Testing

### Manual Testing

1. **Start both applications**:
   ```bash
   # Terminal 1 - Backend API
   cd OrchidsShop.API
   dotnet run
   
   # Terminal 2 - Frontend
   cd OrchidsShop.PresentationLayer
   dotnet run
   ```

2. **Test customer flow**:
   - Browse orchids
   - Add items to cart
   - Place orders
   - View order history

3. **Test admin flow**:
   - Login as admin
   - Manage orchids
   - Manage orders
   - Manage accounts

4. **Test API endpoints**:
   - Use the provided HTTP client collection
   - Test all CRUD operations
   - Verify error handling

### API Testing with HTTP Client

Use the provided HTTP client collection (`OrchidsShop.API.http`) to test all endpoints:

```http
### Get all orchids
GET http://localhost:5077/api/orchids?pageNumber=1&pageSize=10

### Get categories
GET http://localhost:5077/api/categories

### Login user
POST http://localhost:5077/api/accounts/login
Content-Type: application/json

{
  "email": "admin@orchidshop.com",
  "password": "Admin123!"
}
```

## Troubleshooting

### Common Issues

1. **JSON Deserialization Errors**
   - **Cause**: API response format mismatch
   - **Solution**: Use the correct API helper method (`GetAsync`, `GetBllAsync`, `GetArrayAsync`)

2. **Authentication Errors**
   - **Cause**: Missing or invalid JWT token
   - **Solution**: Ensure user is logged in and session is active

3. **CORS Errors**
   - **Cause**: Cross-origin requests blocked
   - **Solution**: Configure CORS in the API project

4. **Connection Errors**
   - **Cause**: API not running or wrong URL
   - **Solution**: Ensure API is running on correct port and URL is configured

### Debug Tips

1. **Check API Response**:
   ```csharp
   var responseData = await response.Content.ReadAsStringAsync();
   _logger.LogInformation("API Response: {Response}", responseData);
   ```

2. **Verify Configuration**:
   ```csharp
   _logger.LogInformation("API Base URL: {BaseUrl}", StringValue.BaseUrl);
   ```

3. **Check Session Data**:
   ```csharp
   var jwtToken = HttpContext.Session.GetString("JwtToken");
   _logger.LogInformation("JWT Token: {Token}", jwtToken);
   ```

## Performance Optimization

### Parallel Loading

Load multiple API calls in parallel for better performance:

```csharp
var tasks = new[]
{
    LoadOrchidsAsync(),
    LoadCategoriesAsync(),
    LoadStatisticsAsync()
};

await Task.WhenAll(tasks);
```

### Caching

Implement response caching for frequently accessed data:

```csharp
// Cache categories for 5 minutes
var cacheKey = "categories";
var categories = await _cache.GetOrCreateAsync(cacheKey, async entry =>
{
    entry.AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(5);
    var response = await _categoryService.GetCategoriesAsync();
    return response?.Data ?? new List<CategoryModel>();
});
```

## 🎥 Demo Video
**Watch the API integration in action:** [OrchidsShop Demo](https://youtu.be/-Gb_q9g5eVs)

---

**✅ All API integrations tested and working with real backend endpoints** 