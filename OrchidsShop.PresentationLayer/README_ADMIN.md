# OrchidsShop Admin Documentation

This document provides comprehensive information about the OrchidsShop admin functionality, including setup, usage, and technical details.

## Overview

The OrchidsShop admin system provides a complete management interface for administrators to manage orchids, orders, accounts, view statistics, and control the shop's inventory. The admin functionality is built using ASP.NET Razor Pages with a modern, responsive design.

**✅ Features:**
- Complete CRUD operations for orchids (Create, Read, Update)
- Order management with status updates
- Account management and user control
- Real-time search and filtering
- Statistics dashboard
- Responsive design for all devices
- Session-based authentication
- Form validation and error handling

## 🎥 Demo Video
**Watch the admin functionality in action:** [OrchidsShop Demo](https://youtu.be/-Gb_q9g5eVs)

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

### 2. Order Management (`/Admin/Orders`)

**Purpose**: Manage all customer orders and update order status.

**Features**:
- **Order Table**: List all orders with customer information
- **Status Updates**: Update order status via AJAX calls
- **Order Statistics**: Overview of order metrics
- **Filtering**: Filter orders by status, date, and customer
- **Real-time Updates**: Status changes without page refresh

**URL**: `/Admin/Orders`

**Access**: Admin role required

**Order Statuses**:
- **Pending**: Order placed but not yet processed
- **Processing**: Order is being prepared
- **Shipped**: Order has been shipped
- **Delivered**: Order has been delivered
- **Cancelled**: Order has been cancelled

### 3. Account Management (`/Admin/Accounts`)

**Purpose**: View and manage user accounts in the system.

**Features**:
- **Account Table**: List all user accounts with roles
- **Role Management**: View user roles and permissions
- **Account Statistics**: Overview of user metrics
- **Search & Filter**: Find specific users quickly
- **User Details**: View detailed account information

**URL**: `/Admin/Accounts`

**Access**: Admin role required

### 4. Create Orchid (`/Admin/CreateOrchid`)

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

### 5. Edit Orchid (`/Admin/EditOrchid/{id}`)

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

### Order Status Management

The order management page includes AJAX-based status updates:

```javascript
function updateOrderStatusSubmit() {
    const orderId = $('#orderId').val();
    const status = $('#orderStatus').val();
    
    if (!orderId || !status) {
        alert('Please fill in all required fields');
        return;
    }
    
    // Show loading state
    const submitBtn = $('#updateStatusForm button[type="submit"]');
    const originalText = submitBtn.html();
    submitBtn.html('<span class="spinner-border spinner-border-sm me-2"></span>Updating...');
    submitBtn.prop('disabled', true);
    
    // Make API call to update order status
    fetch('/Admin/Orders?handler=UpdateStatus', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json',
            'RequestVerificationToken': document.querySelector('input[name="__RequestVerificationToken"]').value
        },
        body: JSON.stringify({
            orderId: orderId,
            status: status
        })
    })
    .then(response => response.json())
    .then(data => {
        if (data.success) {
            showAlert('success', data.message || 'Order status updated successfully!');
            $('#updateStatusModal').modal('hide');
            // Refresh the page to show updated status
            location.reload();
        } else {
            showAlert('danger', data.message || 'Failed to update order status');
        }
    })
    .catch(error => {
        console.error('Error:', error);
        showAlert('danger', 'An error occurred while updating order status');
    })
    .finally(() => {
        submitBtn.prop('disabled', false).html(originalText);
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
public async Task<IActionResult> OnPostAsync()
{
    if (!ModelState.IsValid)
    {
        return Page();
    }

    try
    {
        var response = await _orchidService.CreateOrchidAsync(orchidRequest);
        
        if (response?.Success == true)
        {
            TempData["SuccessMessage"] = "Orchid created successfully!";
            return RedirectToPage("/Admin/Orchids");
        }
        else
        {
            ModelState.AddModelError("", response?.Message ?? "Failed to create orchid");
            return Page();
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error creating orchid");
        ModelState.AddModelError("", "An error occurred while creating the orchid");
        return Page();
    }
}
```

## Technical Implementation

### API Integration

The admin functionality integrates with the backend API using dedicated services:

```csharp
// Admin order management with IsManagment parameter
public async Task<ApiResponse<List<OrderModel>>?> GetOrdersForAdminAsync(OrderQueryModel? queryModel = null)
{
    var adminQueryModel = queryModel ?? new OrderQueryModel();
    adminQueryModel.IsManagment = true; // Set to true for admin management
    
    return await GetOrdersAsync(adminQueryModel);
}
```

### Session Management

Admin sessions are managed securely:

```csharp
// Check admin privileges
public async Task<IActionResult> OnGetAsync()
{
    var userRole = HttpContext.Session.GetString("UserRole");
    if (string.IsNullOrEmpty(userRole) || userRole != "ADMIN")
    {
        TempData["ErrorMessage"] = "Access denied. Admin privileges required.";
        return RedirectToPage("/Auth/Login");
    }
    
    await LoadDataAsync();
    return Page();
}
```

### Error Handling

Comprehensive error handling throughout the admin interface:

```csharp
private async Task LoadOrchidsAsync()
{
    try
    {
        var response = await _orchidService.GetOrchidsAsync(new OrchidQueryModel
        {
            PageSize = 100,
            SortColumn = "Name",
            SortDir = "Asc"
        });

        if (response?.Success == true && response.Data != null)
        {
            Orchids = response.Data;
        }
        else
        {
            _logger.LogWarning("Failed to load orchids. Message: {Message}", response?.Message);
            Orchids = new List<OrchidModel>();
        }
    }
    catch (Exception ex)
    {
        _logger.LogError(ex, "Error loading orchids for admin");
        Orchids = new List<OrchidModel>();
    }
}
```

## Troubleshooting

### Common Issues

1. **Access Denied Errors**
   - **Cause**: User not logged in or not admin role
   - **Solution**: Ensure user is logged in with admin credentials

2. **API Connection Errors**
   - **Cause**: Backend API not running
   - **Solution**: Start the OrchidsShop.API project

3. **Form Validation Errors**
   - **Cause**: Invalid form data
   - **Solution**: Check form validation messages and required fields

4. **Session Timeout**
   - **Cause**: Session expired (30-minute timeout)
   - **Solution**: Re-login to admin account

### Debug Tips

1. **Check Session Data**:
   ```csharp
   var userRole = HttpContext.Session.GetString("UserRole");
   var userEmail = HttpContext.Session.GetString("UserEmail");
   _logger.LogInformation("User: {Email}, Role: {Role}", userEmail, userRole);
   ```

2. **Verify API Responses**:
   ```csharp
   _logger.LogInformation("API Response: {Response}", JsonSerializer.Serialize(response));
   ```

3. **Check Configuration**:
   ```csharp
   _logger.LogInformation("API Base URL: {BaseUrl}", StringValue.BaseUrl);
   ```

## Future Enhancements

### Planned Features

1. **Delete Functionality**
   - Add orchid deletion with confirmation dialogs
   - Soft delete for data integrity

2. **Bulk Operations**
   - Bulk category assignment
   - Bulk price updates
   - Bulk status updates for orders

3. **Advanced Analytics**
   - Sales analytics dashboard
   - Customer insights
   - Inventory tracking

4. **Image Management**
   - File upload for orchid images
   - Image optimization and resizing
   - Multiple image support

5. **Enhanced Security**
   - JWT token authentication
   - Two-factor authentication
   - Role-based permissions

6. **Real-time Updates**
   - SignalR integration for live updates
   - Real-time notifications
   - Live order tracking

### Performance Improvements

1. **Caching**
   - Response caching for frequently accessed data
   - Memory caching for statistics

2. **Pagination Optimization**
   - Virtual scrolling for large datasets
   - Lazy loading for images

3. **API Optimization**
   - Batch API calls
   - Optimized queries

## 🎥 Demo Video
**Watch the admin functionality in action:** [OrchidsShop Demo](https://youtu.be/-Gb_q9g5eVs)

---

**✅ All admin features tested and working with real backend API** 