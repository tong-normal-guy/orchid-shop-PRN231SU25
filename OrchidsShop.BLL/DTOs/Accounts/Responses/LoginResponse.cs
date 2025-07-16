namespace OrchidsShop.BLL.DTOs.Accounts.Responses;

public class LoginResponse
{
    public string? Token { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Role { get; set; }
    public Guid? UserId { get; set; }
} 