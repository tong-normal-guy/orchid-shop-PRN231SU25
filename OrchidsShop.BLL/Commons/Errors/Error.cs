using OrchidsShop.BLL.Commons.Results;

namespace OrchidsShop.BLL.Commons.Errors;


public class Error
{
    public StatusCode Code { get; set; }
    public string Message { get; set; }
}