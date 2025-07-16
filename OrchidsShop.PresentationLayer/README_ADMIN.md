# OrchidsShop Admin Documentation

This document provides comprehensive information about the OrchidsShop admin functionality, including setup, usage, and technical details.

## Overview

The OrchidsShop admin system provides a complete management interface for administrators to manage orchids, view statistics, and control the shop's inventory. The admin functionality is built using ASP.NET Razor Pages with a modern, responsive design.

**✅ Features:**
- Complete CRUD operations for orchids (Create, Read, Update)
- Real-time search and filtering
- Statistics dashboard
- Responsive design for all devices
- Session-based authentication
- Form validation and error handling

## Table of Contents

1. [Setup & Configuration](#setup--configuration)
2. [Admin Access](#admin-access)
3. [Admin Pages](#admin-pages)
4. [Features & Functionality](#features--functionality)
5. [Technical Implementation](#technical-implementation)
6. [Troubleshooting](#troubleshooting)
7. [Future Enhancements](#future-enhancements)

## Setup & Configuration

### Prerequisites

1. **Backend API Running**: Ensure OrchidsShop.API is running on `http://localhost:5077`
2. **Database Setup**: Ensure the database is properly configured and seeded
3. **Admin Account**: Create an admin account in the system

### Required Services

The admin functionality requires these services to be registered in `Program.cs`:

```csharp
// Register API services
services.AddHttpClient<ApiHelper>();
services.AddScoped<ApiHelper>();
services.AddScoped<CategoryApiService>();
services.AddScoped<OrchidApiService>();
services.AddScoped<AccountApiService>();
services.AddScoped<OrderApiService>();

// Register session services
services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

### Configuration

Ensure the API base URL is correctly configured in `Constants/StringValue.cs`:

```csharp
public static class StringValue
{
    public const string BaseUrl = "http://localhost:5077/api/";
}
```

## Admin Access

### Authentication

Admin access is controlled through session-based authentication. Users must:

1. **Login** with admin credentials
2. **Have ADMIN role** assigned to their account
3. **Maintain active session** (30-minute timeout)

### Login Process

1. Navigate to `/Auth/Login`
2. Enter admin credentials:
   - Email: `admin@orchidshop.com`
   - Password: `Admin123!`
3. Upon successful login, the session stores:
   - `UserEmail`: User's email address
   - `UserRole`: User's role (must be "ADMIN")

### Access Control

All admin pages check for admin privileges:

```csharp
var userRole = HttpContext.Session.GetString("UserRole");
if (string.IsNullOrEmpty(userRole) || userRole != "ADMIN")
{
    TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
    return RedirectToPage("/Auth/Login");
}
```

## Admin Pages

### 1. Admin Dashboard (`/Admin/Orchids`)

**Purpose**: Main admin interface for managing orchids and viewing statistics.

**Features**:
- **Statistics Cards**: Display total orchids, categories, orders, and customers
- **Orchid Table**: List all orchids with search and filtering
- **Quick Actions**: Edit and view buttons for each orchid
- **Responsive Design**: Works on desktop, tablet, and mobile

**URL**: `/Admin/Orchids`

**Access**: Admin role required

### 2. Create Orchid (`/Admin/CreateOrchid`)

**Purpose**: Form to create new orchids in the system.

**Features**:
- **Form Validation**: Client-side and server-side validation
- **Real-time Preview**: Live preview of orchid information
- **Category Selection**: Dropdown with all available categories
- **Image URL Support**: Optional image URL for orchid display
- **Type Selection**: Natural or Artificial orchid types

**URL**: `/Admin/CreateOrchid`

**Access**: Admin role required

**Form Fields**:
- **Name** (required): Orchid name
- **Category** (required): Orchid category
- **Price** (required): Orchid price (decimal)
- **Type** (required): Natural or Artificial
- **Description** (optional): Detailed description
- **Image URL** (optional): URL for orchid image

### 3. Edit Orchid (`/Admin/EditOrchid/{id}`)

**Purpose**: Form to edit existing orchids in the system.

**Features**:
- **Pre-populated Form**: Loads existing orchid data
- **Form Validation**: Client-side and server-side validation
- **Real-time Preview**: Live preview of changes
- **Category Selection**: Dropdown with all available categories
- **Current Information Display**: Shows current orchid details

**URL**: `/Admin/EditOrchid/{id}` (where {id} is the orchid's GUID)

**Access**: Admin role required

**Form Fields**: Same as Create Orchid form

## Features & Functionality

### Search & Filtering

The admin dashboard includes powerful search and filtering capabilities:

```javascript
// Search by orchid name
$('#searchInput').on('input', function() {
    filterOrchids();
});

// Filter by category
$('#categoryFilter').on('change', function() {
    filterOrchids();
});

function filterOrchids() {
    const searchTerm = $('#searchInput').val().toLowerCase();
    const categoryFilter = $('#categoryFilter').val();
    
    $('#orchidsTableBody tr').each(function() {
        const row = $(this);
        const name = row.find('td:nth-child(2) strong').text().toLowerCase();
        const category = row.find('td:nth-child(3)').text();
        
        const matchesSearch = name.includes(searchTerm);
        const matchesCategory = !categoryFilter || category === categoryFilter;
        
        if (matchesSearch && matchesCategory) {
            row.show();
        } else {
            row.hide();
        }
    });
}
```

### Statistics Dashboard

The admin dashboard displays key metrics:

- **Total Orchids**: Count of all orchids in the system
- **Categories**: Count of all orchid categories
- **Total Orders**: Count of all orders
- **Customers**: Count of registered customers

Statistics are loaded in parallel for optimal performance:

```csharp
private async Task LoadStatisticsAsync()
{
    try
    {
        // Load statistics in parallel
        var orchidsTask = _orchidService.GetOrchidsAsync(new OrchidQueryModel { PageSize = 1 });
        var categoriesTask = _categoryService.GetCategoriesAsync(new CategoryQueryModel { PageSize = 1 });
        var ordersTask = _orderService.GetOrdersAsync(new OrderQueryModel { PageSize = 1 });

        await Task.WhenAll(orchidsTask, categoriesTask, ordersTask);

        // Set statistics from pagination metadata
        if (orchidsTask.Result?.Pagination != null)
            TotalOrchids = orchidsTask.Result.Pagination.TotalRecords;

        if (categoriesTask.Result?.Pagination != null)
            TotalCategories = categoriesTask.Result.Pagination.TotalRecords;

        if (ordersTask.Result?.Pagination != null)
            TotalOrders = ordersTask.Result.Pagination.TotalRecords;
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading statistics for admin");
    }
}
```

### Form Validation

Both create and edit forms include comprehensive validation:

**Client-side Validation**:
- Required field checking
- Real-time validation feedback
- Visual indicators (red/green borders)

**Server-side Validation**:
- Model state validation
- Business rule validation
- API response validation

```csharp
// Server-side validation example
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
```

### Real-time Preview

The create and edit forms include real-time preview functionality:

```javascript
function updatePreview() {
    const name = $('#Orchid_Name').val();
    const categoryId = $('#Orchid_CategoryId').val();
    const price = $('#Orchid_Price').val();
    const type = $('#Orchid_IsNatural').val();
    
    // Update type preview
    if (type === 'true') {
        $('#previewType').text('Natural').removeClass('text-muted').addClass('text-success');
    } else if (type === 'false') {
        $('#previewType').text('Artificial').removeClass('text-muted').addClass('text-info');
    } else {
        $('#previewType').text('Not selected').removeClass('text-success text-info').addClass('text-muted');
    }
    
    // Update category preview
    if (categoryId) {
        const categoryName = $('#Orchid_CategoryId option:selected').text();
        $('#previewCategory').text(categoryName).removeClass('text-muted').addClass('text-primary');
    } else {
        $('#previewCategory').text('Not selected').removeClass('text-primary').addClass('text-muted');
    }
    
    // Update price preview
    if (price && !isNaN(price)) {
        $('#previewPrice').text('$' + parseFloat(price).toFixed(2));
    } else {
        $('#previewPrice').text('$0.00');
    }
}
```

## Technical Implementation

### Page Models

Each admin page has a corresponding page model that handles:

- **Authentication**: Checking admin privileges
- **Data Loading**: Loading orchids, categories, and statistics
- **Form Processing**: Handling form submissions
- **Validation**: Server-side validation
- **Error Handling**: Comprehensive error handling and logging

### API Integration

Admin pages use the API services to communicate with the backend:

```csharp
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
}
```

### Model Binding

The admin forms use proper model binding with separate display and request models:

```csharp
// Display model for showing orchid information
public OrchidModel OrchidDisplay { get; set; } = new();

// Request model for form binding
[BindProperty]
public OrchidRequestModel Orchid { get; set; } = new();
```

### Error Handling

Comprehensive error handling is implemented throughout:

```csharp
try
{
    var response = await _orchidService.UpdateOrchidAsync(orchidRequest);
    
    if (response?.Success == true)
    {
        _logger.LogInformation("Successfully updated orchid with ID: {Id}", Orchid.Id);
        TempData["SuccessMessage"] = "Orchid updated successfully!";
        return RedirectToPage("/Admin/Orchids");
    }
    else
    {
        _logger.LogWarning("Failed to update orchid. Message: {Message}", response?.Message);
        TempData["ErrorMessage"] = response?.Message ?? "Failed to update orchid.";
        return Page();
    }
}
catch (Exception ex)
{
    _logger.LogError(ex, "Error updating orchid");
    TempData["ErrorMessage"] = "An error occurred while updating the orchid. Please try again.";
    return Page();
}
```

### Responsive Design

The admin interface is fully responsive with:

- **Bootstrap 5**: Modern CSS framework
- **Custom CSS**: Gradient headers and modern styling
- **Mobile-first**: Optimized for all screen sizes
- **Touch-friendly**: Large buttons and touch targets

## Troubleshooting

### Common Issues

#### 1. Access Denied Error

**Problem**: "Access denied. Admin privileges required."

**Solution**:
1. Ensure you're logged in with admin credentials
2. Check that your account has ADMIN role
3. Clear browser session and login again
4. Verify session configuration in `Program.cs`

#### 2. API Connection Issues

**Problem**: "Failed to load orchids" or similar API errors.

**Solution**:
1. Ensure OrchidsShop.API is running on `http://localhost:5077`
2. Check network connectivity
3. Verify API base URL in `StringValue.cs`
4. Check API logs for errors

#### 3. Form Validation Errors

**Problem**: Form submissions fail with validation errors.

**Solution**:
1. Ensure all required fields are filled
2. Check field formats (price must be positive number)
3. Verify category selection
4. Check browser console for JavaScript errors

#### 4. Image Loading Issues

**Problem**: Orchid images don't load properly.

**Solution**:
1. Verify image URLs are valid
2. Check CORS settings if using external images
3. Ensure default image path exists: `/assets/logos/orchid-openai.png`
4. Check browser network tab for image loading errors

### Debugging

#### Enable Detailed Logging

Add detailed logging to troubleshoot issues:

```csharp
// In appsettings.Development.json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning",
      "OrchidsShop.PresentationLayer": "Debug"
    }
  }
}
```

#### Check API Responses

Use browser developer tools to inspect API responses:

1. Open browser developer tools (F12)
2. Go to Network tab
3. Perform admin actions
4. Check API request/response details

#### Verify Session Data

Check session data in browser developer tools:

1. Open browser developer tools (F12)
2. Go to Application tab
3. Check Session Storage for user data
4. Verify UserRole is set to "ADMIN"

## Future Enhancements

### Planned Features

1. **Delete Functionality**
   - Add delete buttons to orchid list
   - Implement soft delete with confirmation
   - Add bulk delete operations

2. **Image Upload**
   - File upload for orchid images
   - Image resizing and optimization
   - Cloud storage integration

3. **Bulk Operations**
   - Bulk category assignment
   - Bulk price updates
   - Import/export functionality

4. **Advanced Filtering**
   - Date range filtering
   - Price range filtering
   - Status-based filtering

5. **User Management**
   - Admin user management interface
   - Role assignment and management
   - User activity logging

6. **Analytics Dashboard**
   - Sales analytics
   - Popular orchids tracking
   - Customer behavior insights

### Technical Improvements

1. **Performance Optimization**
   - Implement caching for frequently accessed data
   - Add pagination for large datasets
   - Optimize database queries

2. **Security Enhancements**
   - Implement JWT tokens
   - Add two-factor authentication
   - Enhance session security

3. **UI/UX Improvements**
   - Add dark mode support
   - Implement drag-and-drop functionality
   - Add keyboard shortcuts

4. **API Enhancements**
   - Add real-time updates with SignalR
   - Implement API versioning
   - Add rate limiting

## Support

For technical support or questions about the admin functionality:

1. **Check Logs**: Review application logs for error details
2. **API Documentation**: Refer to `README_API_INTEGRATION.md`
3. **Database**: Verify database connectivity and data integrity
4. **Network**: Ensure proper network configuration

## Conclusion

The OrchidsShop admin system provides a comprehensive, user-friendly interface for managing orchid inventory. With its modern design, robust validation, and responsive layout, it offers administrators a powerful tool for maintaining the shop's product catalog.

The system is built with scalability in mind, making it easy to add new features and enhance existing functionality as the business grows. 