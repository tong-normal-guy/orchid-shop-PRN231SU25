using System.ComponentModel.DataAnnotations;

namespace OrchidsShop.PresentationLayer.Models.Accounts;

/// <summary>
/// Account response model matching QueryAccountResponse from BLL
/// </summary>
public class AccountModel
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
}

/// <summary>
/// Role response model for API responses
/// </summary>
public class RoleModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Account request model for creating/updating accounts
/// </summary>
public class AccountRequestModel
{
    public Guid? Id { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public string? Role { get; set; }
}

/// <summary>
/// Login request model
/// </summary>
public class LoginRequestModel
{
    [Required(ErrorMessage = "Email is required")]
    /*[EmailAddress(ErrorMessage = "Please enter a valid email address")]*/
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }
}

/// <summary>
/// Registration request model
/// </summary>
public class RegisterRequestModel
{
    [Required(ErrorMessage = "Full name is required")]
    [StringLength(100, ErrorMessage = "Name cannot be longer than 100 characters")]
    [Display(Name = "Full Name")]
    public string? Name { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress(ErrorMessage = "Please enter a valid email address")]
    [Display(Name = "Email Address")]
    public string? Email { get; set; }

    [Required(ErrorMessage = "Password is required")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Password must be at least 6 characters long")]
    [DataType(DataType.Password)]
    [Display(Name = "Password")]
    public string? Password { get; set; }

    [Required(ErrorMessage = "Password confirmation is required")]
    [DataType(DataType.Password)]
    [Display(Name = "Confirm Password")]
    [Compare("Password", ErrorMessage = "The password and confirmation password do not match")]
    public string? ConfirmPassword { get; set; }

    [Required(ErrorMessage = "Please select an account type")]
    [Display(Name = "Account Type")]
    public string? Role { get; set; }
}

/// <summary>
/// Account query model for filtering and pagination
/// </summary>
public class AccountQueryModel
{
    public string? Search { get; set; }
    public string? Roles { get; set; }
    public string? Email { get; set; }
    /// <summary>
    /// Comma-separated list of account IDs for filtering
    /// </summary>
    public string? Ids { get; set; }
    public int PageNumber { get; set; } = 1;
    public int PageSize { get; set; } = 10;
    public string? SortColumn { get; set; }
    public string? SortDir { get; set; }
} 