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

    public OrchidsController(IOrchidService orchidService)
    {
        _orchidService = orchidService;
    }

    [HttpGet]
    [SwaggerOperation(
        Summary = "Get Orchids",
        Description = "Retrieves a list of orchids based on the provided query parameters."
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
