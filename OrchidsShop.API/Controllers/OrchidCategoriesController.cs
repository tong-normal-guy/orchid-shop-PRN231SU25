using Microsoft.AspNetCore.Mvc;
using OrchidsShop.BLL.DTOs.Categories.Requests;
using OrchidsShop.BLL.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace OrchidsShop.API.Controllers;

/// <summary>
/// This controller handles requests **READ/QUERY WITH QUERY PARAMETER** related to orchid categories.
/// </summary>
[ApiController]
[Route("api/orchid-categories")]
public class OrchidCategoriesController : Controller
{
    private readonly OrchidCategoryService _service; 
    public OrchidCategoriesController(OrchidCategoryService service)
    {
        _service = service;
    }
    
    [HttpGet]
    [SwaggerOperation(
        Summary = "Truy vấn danh sách danh mục hoa lan",
        Description = "Truy xuất danh sách các danh mục hoa lan với khả năng lọc, tìm kiếm, phân trang và sắp xếp. " +
                    "Endpoint thống nhất cho tất cả các thao tác truy vấn danh mục." +
                    "\n\n**Tham số truy vấn:**" +
                    "\n- search: string (tìm kiếm trong tên danh mục)" +
                    "\n- ids: List<string> (danh sách ID danh mục cụ thể)" +
                    "\n- pageNumber: int (số trang, mặc định 1)" +
                    "\n- pageSize: int (kích thước trang, mặc định 10)" +
                    "\n- sortColumn: string (cột sắp xếp, ví dụ: Name)" +
                    "\n- sortDir: string (hướng sắp xếp: Asc/Desc)" +
                    "\n\n**Ví dụ sử dụng:**" +
                    "\n- Tìm kiếm: ?search=Phal" +
                    "\n- Lọc theo ID: ?ids=guid1,guid2" +
                    "\n- Sắp xếp: ?sortColumn=Name&sortDir=Asc" +
                    "\n- Phân trang: ?pageNumber=2&pageSize=5" +
                    "\n\n**Trả về:** Dữ liệu với metadata phân trang",
        OperationId = "GetOrchidCategories",
        Tags = new[] { "Orchid Categories" }
    )]
    public async Task<IActionResult> GetOrchidCategories([FromQuery] QueryCategoryRequest request)
    {
        var result = await _service.QueryOrchidCategoriesAsync(request);
        return result.IsError
            ? BadRequest(new { 
                message = result.Message, 
                errors = result.Errors 
            })
            : Ok(new { 
                message = result.Message, 
                data = result.Payload,
                pagination = result.MetaData 
            });
    }
}