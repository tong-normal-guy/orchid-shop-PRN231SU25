namespace OrchidsShop.BLL.DTOs.Accounts.Requests;

public class CommandAccountRequest
{
    public Guid? Id { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Password { get; set; }
    public string? ConfirmPassword { get; set; }
    public string? Role { get; set; }
}