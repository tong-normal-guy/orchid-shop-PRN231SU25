# OrchidsShop - ASP.NET Razor Pages Application

A modern e-commerce application for selling orchids, built with ASP.NET Razor Pages, featuring a clean UI, RESTful API integration, and comprehensive admin functionality.

## 🎥 Demo Video
**Watch the application in action:** [OrchidsShop Demo](https://youtu.be/-Gb_q9g5eVs)

## 🌟 Features

### Customer Features
- **Product Catalog**: Browse orchids with advanced filtering and search
- **Shopping Cart**: Add items and manage quantities
- **Order Management**: View order history and track status
- **User Authentication**: Secure login and registration system
- **Account Management**: View and edit profile information
- **Responsive Design**: Works perfectly on desktop, tablet, and mobile

### 🆕 Admin Features
- **Admin Dashboard**: Complete overview with statistics and orchid management
- **CRUD Operations**: Create, read, and update orchids (delete functionality planned)
- **Order Management**: View and manage all customer orders with status updates
- **Account Management**: View and manage user accounts
- **Real-time Search**: Filter orchids by name and category
- **Form Validation**: Comprehensive client-side and server-side validation
- **Session-based Security**: Admin-only access with role-based authentication

## 🏗️ Architecture

```
OrchidsShop/
├── OrchidsShop.API/           # Backend REST API
├── OrchidsShop.BLL/           # Business Logic Layer
├── OrchidsShop.DAL/           # Data Access Layer
└── OrchidsShop.PresentationLayer/  # Frontend Razor Pages
    ├── Pages/
    │   ├── Admin/             # 🆕 Admin management pages
    │   │   ├── Orchids.cshtml         # Orchid management dashboard
    │   │   ├── Orders.cshtml          # Order management
    │   │   ├── Accounts.cshtml        # Account management
    │   │   ├── CreateOrchid.cshtml    # Create new orchids
    │   │   └── EditOrchid.cshtml      # Edit existing orchids
    │   ├── Auth/              # Authentication pages
    │   ├── Orders/            # Order management
    │   └── Shared/            # Layout and components
    ├── Services/              # API integration services
    └── Models/                # Data models and DTOs
```

## 🚀 Quick Start

### Prerequisites
- .NET 8.0 SDK
- SQL Server (or SQL Server Express)
- Visual Studio 2022 or VS Code

### Setup Instructions

1. **Clone the repository**
   ```bash
   git clone <repository-url>
   cd orchid-shop-PRN231SU25
   ```

2. **Configure the database**
   - Update connection strings in `appsettings.json`
   - Run database migrations (if applicable)

3. **Start the backend API**
   ```bash
   cd OrchidsShop.API
   dotnet run
   ```

4. **Start the frontend application**
   ```bash
   cd OrchidsShop.PresentationLayer
   dotnet run
   ```

5. **Access the application**
   - Frontend: http://localhost:5081
   - Backend API: http://localhost:5077

### Admin Access

To access the admin functionality:

1. **Login** with admin credentials:
   - Email: `admin@orchidshop.com`
   - Password: `Admin123!`

2. **Navigate** to admin pages:
   - `/Admin/Orchids` - Manage orchids
   - `/Admin/Orders` - Manage orders
   - `/Admin/Accounts` - Manage user accounts

3. **Manage the system** using the comprehensive admin interface

## 📚 Documentation

- **[API Integration Guide](OrchidsShop.PresentationLayer/README_API_INTEGRATION.md)** - Complete guide to API services and integration
- **[Admin Documentation](OrchidsShop.PresentationLayer/README_ADMIN.md)** - Comprehensive admin functionality guide
- **[API Testing](OrchidsShop.API/OrchidsShop.API.http)** - HTTP client collection for API testing

## 🛠️ Technology Stack

### Backend
- **ASP.NET Core 8.0** - Web API framework
- **Entity Framework Core** - ORM for data access
- **AutoMapper** - Object mapping
- **Carter** - Minimal API framework for endpoints
- **JWT Authentication** - Token-based authentication

### Frontend
- **ASP.NET Razor Pages** - Server-side rendering
- **Bootstrap 5** - CSS framework
- **jQuery** - JavaScript library
- **Session Management** - User authentication and authorization

### Architecture Patterns
- **Repository Pattern** - Data access abstraction
- **Unit of Work** - Transaction management
- **Service Layer** - Business logic encapsulation
- **DTO Pattern** - Data transfer objects

## 🔧 Configuration

### API Base URL
Configure the API base URL in `OrchidsShop.PresentationLayer/Constants/StringValue.cs`:

```csharp
public const string BaseUrl = "http://localhost:5077/api/";
```

### Session Configuration
Session services are configured in `Program.cs`:

```csharp
services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
```

## 🎨 UI/UX Features

### Responsive Design
- Mobile-first approach
- Bootstrap 5 grid system
- Touch-friendly interface
- Cross-browser compatibility

### Modern Interface
- Gradient headers and modern styling
- Loading states and animations
- Real-time form validation
- Toast notifications for user feedback

### Admin Interface
- Clean, professional dashboard
- Statistics cards with icons
- Search and filtering capabilities
- Form preview functionality
- Order status management
- User account management

## 🔒 Security

### Authentication
- Session-based authentication
- Role-based access control
- Secure password handling
- Session timeout management

### Admin Security
- Admin-only page access
- Form validation and sanitization
- CSRF protection
- Secure API communication

## 📊 API Integration

The application integrates with a RESTful API that provides:

- **Categories API**: CRUD operations for orchid categories
- **Orchids API**: Advanced filtering and search for orchids
- **Orders API**: Order management and status tracking with admin management
- **Accounts API**: User authentication and management

All API calls include:
- Error handling and retry logic
- Response caching where appropriate
- Comprehensive logging
- Type-safe data models
- Dual response format support (Categories and BLL formats)

## 🧪 Testing

### API Testing
Use the provided HTTP client collection (`OrchidsShop.API.http`) to test all API endpoints.

### Manual Testing
1. **Customer Flow**: Browse products → Add to cart → Place order
2. **Admin Flow**: Login → Manage orchids → Manage orders → Manage accounts
3. **Authentication**: Test login/logout and session management

## 🚀 Deployment

### Development
- Use HTTP for local development
- Configure connection strings for local database
- Enable detailed logging

### Production
- Configure HTTPS certificates
- Update API base URL to production endpoint
- Set up proper database connection
- Configure logging for production environment

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## 📝 License

This project is licensed under the MIT License - see the LICENSE file for details.

## 🆕 Recent Updates

### Admin Functionality (Latest)
- ✅ Complete admin dashboard with statistics
- ✅ Create orchid form with validation
- ✅ Edit orchid form with pre-populated data
- ✅ Order management with status updates
- ✅ Account management interface
- ✅ Real-time search and filtering
- ✅ Responsive admin interface
- ✅ Session-based admin authentication

### API Integration (Fixed)
- ✅ Fixed account page JSON deserialization issue
- ✅ Dual API response format support (Categories and BLL)
- ✅ Comprehensive error handling
- ✅ Pagination with property mapping
- ✅ Type-safe data models
- ✅ Real-time connectivity

### UI/UX Improvements
- ✅ Modern gradient design
- ✅ Loading states and animations
- ✅ Form validation and preview
- ✅ Mobile-responsive layout
- ✅ User-friendly notifications
- ✅ Account profile management

## 🔮 Future Enhancements

- **Delete Functionality**: Add orchid deletion with confirmation
- **Image Upload**: File upload for orchid images
- **Bulk Operations**: Bulk category assignment and price updates
- **Advanced Analytics**: Sales analytics and customer insights
- **Real-time Updates**: SignalR integration for live updates
- **Enhanced Security**: JWT tokens and two-factor authentication
- **Email Notifications**: Order status updates and confirmations

---

**🎥 Watch the demo:** [OrchidsShop Demo](https://youtu.be/-Gb_q9g5eVs)
