using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchidsShop.BLL.DTOs.Orchids.Requests;
using OrchidsShop.BLL.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace OrchidsShop.API.Controllers;

[Route("api/[controller]")]
[ApiController]
public class OrchidsController : ControllerBase
{
    private readonly IOrchidService _orchidService;
    private const string Tags = "Orchids";

    public OrchidsController(IOrchidService orchidService)
    {
        _orchidService = orchidService;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Lấy danh sách các hoa lan",
        Description = "Truy xuất danh sách các hoa lan dựa trên các tham số truy vấn được cung cấp. " +
                      "Hỗ trợ phân trang, lọc và sắp xếp." +
                      "Dùng chung cho lấy chi tiết hoa lan theo ID, theo tên, tìm kiếm, lọc theo ID và tất cả các thao tác truy vấn khác.",
        OperationId = "GetOrchids",
        Tags = new[] { Tags }
    )]
    public async Task<IActionResult> GetOrchids([FromQuery] QueryOrchidRequest request)
    {
        var result = await _orchidService.QueryOrchidsAsync(request);
        if (result.IsError)
        {
            return BadRequest();
        }

        return Ok(result);
    }
}
