namespace OrchidsShop.BLL.DTOs.Orders.Requests;

public class CommandOrderDetailRequest
{
    /// <summary>
    /// Use when update or delete, not for create
    /// </summary>
    public Guid? Id { get; set; }
    
    /// <summary>
    /// Use when create, not for update or delete
    /// </summary>
    public Guid? OrchidId { get; set; }
    
    public int? Quantity { get; set; }
    public decimal? Price { get; set; }

    /// <summary>
    /// Determine if the order detail is deleted from the order
    /// </summary>
    public bool? IsDeleted { get; set; }
}