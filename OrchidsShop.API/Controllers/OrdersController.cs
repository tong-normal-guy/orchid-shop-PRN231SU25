using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchidsShop.BLL.DTOs.Orders.Requests;
using OrchidsShop.BLL.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace OrchidsShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
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
            Summary = "Lấy danh sách đơn hàng",
            Description = "Lấy danh sách đơn hàng với phân trang, lọc và sắp xếp. " +
                          "Dùng chung cho lấy chi tiết đơn hàng theo ID, lọc theo ID, lọc theo trạng thái, lọc theo ngày tạo, lọc theo ngày tạo.",
            OperationId = "QueryOrders",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> QueryOrders([FromQuery] QueryOrderRequest request)
        {
            var result = await _orderService.QueryOrdersAsync(request);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Tạo đơn hàng",
            Description = "Tạo đơn hàng mới.",
            OperationId = "CreateOrder",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> CreateOrder([FromBody] CommandOrderRequest request)
        {
            var result = await _orderService.CreateOrderAsync(request);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpPut]
        [SwaggerOperation(
            Summary = "Cập nhật đơn hàng",
            Description = "Cập nhật đơn hàng." +
            "Cập nhật trạng thái đơn hàng." + 
            "Cập nhật chi tiết đơn hàng.",
            OperationId = "UpdateOrder",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> UpdateOrder([FromBody] CommandOrderRequest request)
        {
            var result = await _orderService.UpdateOrderAsync(request);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }
    }
}
