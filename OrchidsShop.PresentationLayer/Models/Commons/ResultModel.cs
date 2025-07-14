namespace OrchidsShop.PresentationLayer.Models.Commons;

public class ResultModel
{
    public string? Status { get; set; }
    public string? Message { get; set; }
}

public class ResultModel<T>
{
    public string? Status { get; set; }
    public string? Message { get; set; }
    public T? Data { get; set; }
}