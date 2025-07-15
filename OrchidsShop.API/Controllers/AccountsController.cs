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
        [SwaggerOperation(Summary = "Đăng nhập",
        Description = "Đăng nhập tài khoản, Chỉ chấp nhận mỗi email và password, các trường khác sẽ bị bỏ qua.",
        Tags = new[] { Tags }
        )]
        public async Task<IActionResult> Login([FromBody] CommandAccountRequest request)
        {
            var result = await _service.LoginAsync(request);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpPost("register")]
        [SwaggerOperation(Summary = "Đăng ký tài khoản",
        Description = "Đăng ký tài khoản mới",
        Tags = new[] { Tags }
        )]
        public async Task<IActionResult> Register([FromBody] CommandAccountRequest request)
        {
            var result = await _service.RegisterAsync(request);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpPost]
        [SwaggerOperation(Summary = "Tạo tài khoản",
        Description = "Tạo tài khoản mới, dùng cho admin",
        Tags = new[] { Tags }
        )]
        public async Task<IActionResult> Create([FromBody] CommandAccountRequest request)
        {
            var result = await _service.CreateAsync(request);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpGet("roles")]
        [SwaggerOperation(Summary = "Lấy tất cả vai trò",
        Description = "Lấy tất cả vai trò",
        Tags = new[] { Tags }
        )]
        public async Task<IActionResult> GetAllRoles()
        {
            var result = await _service.GetAllRolesAsync();
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpGet("{id}")]
        [SwaggerOperation(Summary = "Lấy tài khoản theo id",
        Description = "Lấy tài khoản theo id",
        Tags = new[] { Tags }
        )]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpPut("{id}")]
        [SwaggerOperation(Summary = "Cập nhật tài khoản",
        Description = "Cập nhật tài khoản",
        Tags = new[] { Tags }
        )]
        public async Task<IActionResult> Update(Guid id, [FromBody] CommandAccountRequest request)
        {
            var result = await _service.UpdateAsync(id, request);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }
    }
}
