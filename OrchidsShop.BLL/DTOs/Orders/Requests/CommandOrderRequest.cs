namespace OrchidsShop.BLL.DTOs.Orders.Requests;

public class CommandOrderRequest
{
    /// <summary>
    /// Use when update or delete, not for create
    /// </summary>
    public Guid? Id { get; set; }
    
    /// <summary>
    /// Use when create, not for update
    /// </summary>
    public Guid? AccountId { get; set; }
    
    public List<CommandOrderDetailRequest>? OrderDetails { get; set; }
    
    /// <summary>
    /// Use when update or delete, not for create
    /// </summary>
    public string? Status { get; set; }
}