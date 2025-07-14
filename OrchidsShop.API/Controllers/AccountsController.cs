using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using OrchidsShop.BLL.DTOs.Accounts.Requests;
using OrchidsShop.BLL.Services;

namespace OrchidsShop.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class AccountsController : ControllerBase
    {
        private readonly AccountService _service;

        public AccountsController(AccountService service)
        {
            _service = service;
        }
        
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] CommandAccountRequest request)
        {
            var result = await _service.LoginAsync(request);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] CommandAccountRequest request)
        {
            var result = await _service.RegisterAsync(request);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpGet("roles")]
        public async Task<IActionResult> GetAllRoles()
        {
            var result = await _service.GetAllRolesAsync();
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(Guid id)
        {
            var result = await _service.GetByIdAsync(id);
            return result.IsError ? BadRequest(result.Message) : Ok(result.Payload);
        }
    }
}
