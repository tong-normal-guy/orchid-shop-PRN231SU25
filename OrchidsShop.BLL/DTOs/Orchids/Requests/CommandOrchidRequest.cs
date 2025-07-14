namespace OrchidsShop.BLL.DTOs.Orchids.Requests;

public class CommandOrchidRequest
{
    public Guid? Id { get; set; }

    public string? Name { get; set; }

    public string? Description { get; set; }

    public string? Url { get; set; }

    public decimal? Price { get; set; }

    public bool? IsNatural { get; set; }

    public Guid? CategoryId { get; set; }
}