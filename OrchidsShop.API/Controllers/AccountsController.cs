using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchidsShop.BLL.DTOs.Accounts.Requests;
using OrchidsShop.BLL.Services;
using Swashbuckle.AspNetCore.Annotations;

namespace OrchidsShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private const string Tags = "Accounts";
        private readonly AccountService _service;

        public AccountsController(AccountService service)
        {
            _service = service;
        }
        
        [HttpPost("login")]
        [SwaggerOperation(
            Summary = "Đăng nhập tài khoản", 
            Description = "Xác thực người dùng bằng email và mật khẩu. Chỉ các trường email và password được xử lý, các trường khác sẽ bị bỏ qua." +
                         "\n\n**Trường bắt buộc:**" +
                         "\n- email: string (địa chỉ email hợp lệ)" +
                         "\n- password: string (mật khẩu)" +
                         "\n\n**Trả về:** Token xác thực và thông tin người dùng",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> Login([FromBody] CommandAccountRequest request)
        {
            var result = await _service.LoginAsync(request);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpPost("register")]
        [SwaggerOperation(
            Summary = "Đăng ký tài khoản mới", 
            Description = "Tạo tài khoản người dùng mới với thông tin đầy đủ." +
                         "\n\n**Trường bắt buộc:**" +
                         "\n- email: string (địa chỉ email duy nhất)" +
                         "\n- name: string (tên hiển thị)" +
                         "\n- password: string (mật khẩu mạnh)" +
                         "\n- confirmPassword: string (xác nhận mật khẩu)" +
                         "\n- role: string (vai trò người dùng - tùy chọn)" +
                         "\n\n**Trả về:** Thông tin tài khoản đã tạo",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> Register([FromBody] CommandAccountRequest request)
        {
            var result = await _service.RegisterAsync(request);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpPost]
        [SwaggerOperation(
            Summary = "Tạo tài khoản (Admin)", 
            Description = "Tạo tài khoản mới bởi quản trị viên. Có thể chỉ định vai trò cụ thể." +
                         "\n\n**Trường bắt buộc:**" +
                         "\n- email: string (địa chỉ email duy nhất)" +
                         "\n- name: string (tên hiển thị)" +
                         "\n- password: string (mật khẩu)" +
                         "\n- confirmPassword: string (xác nhận mật khẩu)" +
                         "\n- role: string (vai trò được chỉ định)" +
                         "\n\n**Quyền:** Chỉ dành cho quản trị viên",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> Create([FromBody] CommandAccountRequest request)
        {
            var result = await _service.CreateAsync(request);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpGet("roles")]
        [SwaggerOperation(
            Summary = "Lấy danh sách vai trò", 
            Description = "Truy xuất tất cả vai trò có sẵn trong hệ thống để sử dụng trong form đăng ký và quản lý người dùng." +
                         "\n\n**Trả về:** Danh sách các vai trò có sẵn (Admin, Staff, Customer, Manager, v.v.)" +
                         "\n\n**Sử dụng:** Để populate dropdown trong frontend",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> GetAllRoles()
        {
            var result = await _service.GetAllRolesAsync();
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(
            Summary = "Lấy thông tin tài khoản theo ID", 
            Description = "Truy xuất thông tin chi tiết của một tài khoản cụ thể bằng ID." +
                         "\n\n**Tham số:**" +
                         "\n- id: Guid (ID duy nhất của tài khoản)" +
                         "\n\n**Trả về:** Thông tin tài khoản được bao bọc trong danh sách (theo pattern OrchidShop)" +
                         "\n\n**Sử dụng:** Xem chi tiết hồ sơ người dùng, chỉnh sửa thông tin",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(
            Summary = "Cập nhật thông tin tài khoản", 
            Description = "Cập nhật thông tin tài khoản người dùng. Sử dụng ReflectionHelper để cập nhật linh hoạt." +
                         "\n\n**Tham số:**" +
                         "\n- id: Guid (ID tài khoản cần cập nhật)" +
                         "\n\n**Trường có thể cập nhật:**" +
                         "\n- email: string (địa chỉ email mới)" +
                         "\n- name: string (tên hiển thị)" +
                         "\n- password: string (mật khẩu mới)" +
                         "\n- confirmPassword: string (xác nhận mật khẩu)" +
                         "\n- role: string (vai trò mới)" +
                         "\n\n**Trả về:** Kết quả cập nhật (OperationResult<bool>)",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> Update(Guid id, [FromBody] CommandAccountRequest request)
        {
            var result = await _service.UpdateAsync(id, request);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }
    }
}
