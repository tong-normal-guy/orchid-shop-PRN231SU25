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
        Summary = "Lấy danh sách các danh mục hoa lan",
        Description = "Truy xuất danh sách các danh mục hoa lan dựa trên các tham số truy vấn được cung cấp. " +
                    "Hỗ trợ phân trang, lọc và sắp xếp. " +
                    "Dùng chung cho lấy chi tiết danh mục theo ID, theo tên, tìm kiếm, lọc theo ID và tất cả các thao tác truy vấn khác.",
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