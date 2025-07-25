using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchidsShop.BLL.DTOs.Accounts.Requests;
using OrchidsShop.BLL.Services;
using Swashbuckle.AspNetCore.Annotations;
using System.Security.Claims;
using System.IdentityModel.Tokens.Jwt;

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
                         "\n\n**Trả về:** OperationResult với token và thông tin người dùng trong payload",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> Login([FromBody] CommandAccountRequest request)
        {
            var result = await _service.LoginAsync(request);
            
            if (result.IsError)
            {
                return BadRequest(new
                {
                    statusCode = result.StatusCode.ToString(),
                    message = result.Message,
                    isError = true,
                    payload = (object?)null,
                    metaData = (object?)null,
                    errors = result.Errors
                });
            }

            return Ok(new
            {
                statusCode = result.StatusCode.ToString(),
                message = result.Message,
                isError = false,
                payload = result.Payload,
                metaData = (object?)null,
                errors = (object?)null
            });
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

        [HttpGet("profile")]
        [SwaggerOperation(
            Summary = "Lấy thông tin tài khoản hiện tại", 
            Description = "Truy xuất thông tin chi tiết của tài khoản hiện tại từ JWT token." +
                         "\n\n**Yêu cầu:** JWT token hợp lệ trong Authorization header" +
                         "\n\n**Trả về:** Thông tin tài khoản được bao bọc trong danh sách (theo pattern OrchidShop)" +
                         "\n\n**Sử dụng:** Xem chi tiết hồ sơ người dùng hiện tại",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> GetCurrentProfile()
        {
            // Extract user ID from JWT token
            var userClaim = User.FindFirst(ClaimTypes.NameIdentifier);
            if (userClaim == null || !Guid.TryParse(userClaim.Value, out var userId))
            {
                return Unauthorized(new
                {
                    statusCode = "401",
                    message = "Invalid or missing user token",
                    isError = true,
                    payload = (object?)null,
                    metaData = (object?)null,
                    errors = new[] { "User not authenticated or token invalid" }
                });
            }

            var result = await _service.GetCurrentProfileAsync(userId);
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
            var result = await _service.GetCurrentProfileAsync(id);
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

        [HttpPost("roles")]
        [SwaggerOperation(
            Summary = "Tạo vai trò",
            Description = "Tạo vai trò mới trong hệ thống." +
                         "\n\n**Trường bắt buộc:**" +
                         "\n- name: string (tên vai trò)" +
                         "\n\n**Trả về:** Kết quả tạo vai trò (OperationResult<bool>)",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> CreateRole([FromBody] CommandAccountRequest request)
        {
            var result = await _service.CreateRoleAsync(request);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpGet]
        [SwaggerOperation(
            Summary = "Lấy danh sách tài khoản",
            Description = "Lấy danh sách tài khoản từ hệ thống." +
                         "\n\n**Trả về:** Danh sách tài khoản (OperationResult<List<AccountResponse>>)",
            Tags = new[] { Tags }
        )]
        public async Task<IActionResult> GetAllAccounts([FromQuery] QueryAccountRequest request)
        {
            var result = await _service.GetAllAccountsAsync(request);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }
    }
}
