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
        Summary = "Truy vấn danh sách hoa lan với bộ lọc nâng cao",
        Description = "Truy xuất danh sách hoa lan với khả năng lọc, tìm kiếm, phân trang và sắp xếp mạnh mẽ. " +
                      "Endpoint này xử lý tất cả các thao tác truy vấn từ đơn giản đến phức tạp." +
                      "\n\n**Tham số truy vấn:**" +
                      "\n- search: string (tìm kiếm trong tên và mô tả)" +
                      "\n- isNarutal: bool (lọc theo loại tự nhiên/nhân tạo)" +
                      "\n- categories: string (danh sách tên danh mục, phân cách bằng dấu phẩy)" +
                      "\n- minPrice: decimal (giá tối thiểu)" +
                      "\n- maxPrice: decimal (giá tối đa)" +
                      "\n- ids: string (danh sách ID hoa lan, phân cách bằng dấu phẩy)" +
                      "\n- pageNumber: int (số trang, mặc định 1)" +
                      "\n- pageSize: int (kích thước trang, mặc định 10)" +
                      "\n- sortColumn: string (cột sắp xếp)" +
                      "\n- sortDir: string (hướng sắp xếp: Asc/Desc)" +
                      "\n\n**Ví dụ sử dụng:**" +
                      "\n- Tìm kiếm: ?search=beautiful" +
                      "\n- Lọc theo danh mục: ?categories=Phalaenopsis,Cattleya" +
                      "\n- Lọc theo giá: ?minPrice=20&maxPrice=100" +
                      "\n- Kết hợp: ?search=orchid&isNarutal=true&minPrice=25&pageSize=20",
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
