using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchidsShop.BLL.Commons.Results;
using OrchidsShop.BLL.DTOs.Orders.Requests;
using OrchidsShop.BLL.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace OrchidsShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class OrdersController : ControllerBase
    {
        private const string Tags = "Orders";
        private readonly IOrderService _orderService;

        public OrdersController(IOrderService orderService)
        {
            _orderService = orderService;
        }
        
        [HttpGet]
        [SwaggerOperation(
            Summary = "Truy vấn danh sách đơn hàng",
            Description = "Truy xuất danh sách đơn hàng với khả năng lọc, tìm kiếm, phân trang và sắp xếp nâng cao. " +
                          "Hỗ trợ lọc theo trạng thái, khách hàng, ngày tạo và các tiêu chí khác." +
                          "\n\n**Tham số truy vấn:**" +
                          "\n- search: string (tìm kiếm trong thông tin đơn hàng)" +
                          "\n- status: string (lọc theo trạng thái: Pending, Processing, Confirmed, Shipped, Delivered, Cancelled)" +
                          "\n- accountId: Guid (lọc theo khách hàng)" +
                          "\n- fromDate: DateTime (từ ngày)" +
                          "\n- toDate: DateTime (đến ngày)" +
                          "\n- pageNumber: int (số trang)" +
                          "\n- pageSize: int (kích thước trang)" +
                          "\n- sortColumn: string (cột sắp xếp: CreatedDate, Total, Status)" +
                          "\n- sortDir: string (Asc/Desc)" +
                          "\n\n**Sử dụng:** Quản lý đơn hàng, lịch sử mua hàng của khách",
            OperationId = "QueryOrders",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> QueryOrders([FromQuery] QueryOrderRequest request)
        {
            if (request.IsManagment != true)
            {
                // Get user ID from JWT token
            var userId = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userId == null)
            {
                return Unauthorized("User ID not found in token");
            }
            
            if (!Guid.TryParse(userId.Value, out var userGuid))
            {
                return BadRequest("Invalid user ID format in token");
            }
            
            // Set account ID from token
            request.AccountId = userGuid;
            }
            
            // Call service and return result
            var result = await _orderService.QueryOrdersAsync(request);
            
            if (result.IsError)
            {
                return BadRequest(result);
            }
            
            return Ok(result);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Tạo đơn hàng mới",
            Description = "Tạo đơn hàng mới với danh sách sản phẩm. Tự động tính toán tổng tiền và tạo các chi tiết đơn hàng." +
                          "\n\n**Trường bắt buộc:**" +
                          "\n- accountId: Guid (ID khách hàng đặt hàng)" +
                          "\n- orderDetails: List<CommandOrderDetailRequest> (danh sách sản phẩm)" +
                          "\n\n**Cấu trúc OrderDetail:**" +
                          "\n- orchidId: Guid (ID sản phẩm)" +
                          "\n- quantity: int (số lượng, > 0)" +
                          "\n- price: decimal (giá sản phẩm)" +
                          "\n\n**Trả về:** Thông tin đơn hàng đã tạo với trạng thái 'Paid'" +
                          "\n\n**Lưu ý:** Hệ thống sẽ kiểm tra tồn kho và tính toán tổng tiền tự động",
            OperationId = "CreateOrder",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> CreateOrder([FromBody] CommandOrderRequest request)
        {
            var result = await _orderService.CreateOrderAsync(request);
            return result.IsError ? BadRequest(result) : Ok(result);
        }

        [HttpPut]
        [SwaggerOperation(
            Summary = "Cập nhật đơn hàng",
            Description = "Cập nhật thông tin đơn hàng bao gồm trạng thái và chi tiết đơn hàng. Sử dụng ReflectionHelper để cập nhật linh hoạt." +
                          "\n\n**Trường bắt buộc:**" +
                          "\n- id: Guid (ID đơn hàng cần cập nhật)" +
                          "\n\n**Trường có thể cập nhật:**" +
                          "\n- status: string (trạng thái mới: Processing, Confirmed, Shipped, Delivered, Cancelled)" +
                          "\n- orderDetails: List<CommandOrderDetailRequest> (cập nhật chi tiết)" +
                          "\n\n**Cấu trúc OrderDetail cho cập nhật:**" +
                          "\n- id: Guid (để cập nhật chi tiết hiện có)" +
                          "\n- orchidId: Guid (để thêm chi tiết mới)" +
                          "\n- quantity: int (số lượng mới)" +
                          "\n\n**Sử dụng:** Quản lý trạng thái đơn hàng, thay đổi số lượng sản phẩm",
            OperationId = "UpdateOrder",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> UpdateOrder([FromBody] CommandOrderRequest request)
        {
            var result = await _orderService.UpdateOrderAsync(request);
            return result.IsError ? BadRequest(result) : Ok(result);
        }
    }
}
