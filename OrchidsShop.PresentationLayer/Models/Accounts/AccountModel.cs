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
    public string? Email { get; set; }
    public string? Password { get; set; }
}

/// <summary>
/// Registration request model
/// </summary>
public class RegisterRequestModel
{
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public string? Role { get; set; }
} 