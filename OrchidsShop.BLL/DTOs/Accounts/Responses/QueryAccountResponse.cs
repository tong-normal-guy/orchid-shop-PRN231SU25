namespace OrchidsShop.BLL.DTOs.Accounts.Responses;

public class QueryAccountResponse
{
    public Guid? Id { get; set; }
    public string? Name { get; set; }
    public string? Email { get; set; }
    public string? Role { get; set; }
}