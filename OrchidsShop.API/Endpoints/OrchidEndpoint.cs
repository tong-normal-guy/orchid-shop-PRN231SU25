using Carter;
using OrchidsShop.BLL.DTOs.Orchids.Requests;
using OrchidsShop.BLL.Services;

namespace OrchidsShop.API.Endpoints;

public class OrchidEndpoint : ICarterModule
{
    private const string Route = "/api/orchids";
    // private readonly IOrchidService _service;
    //
    // public OrchidEndpoint(IOrchidService service)
    // {
    //     _service = service;
    // }

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // app.MapGet(
        //         Route,
        //         async ([AsParameters]QueryOrchidRequest request, IOrchidService service) =>
        //         {
        //             // Here you would typically handle the request, e.g., query orchids
        //             // For example:
        //             // var result = await orchidService.QueryOrchidsAsync(request);
        //             // return Results.Ok(result);
        //
        //             var result = await service.QueryOrchidsAsync(request);
        //             return result.IsError
        //                 ? Results.BadRequest()
        //                 : Results.Ok(result);
        //         })
            // .WithDisplayName("Get Orchids")
            // .WithDescription("Retrieves a list of orchids based on the provided query parameters.")
            ;
    
        app.MapPost(
                Route,
                async (CommandOrchidRequest request, IOrchidService service) =>
                {
                    var result = await service.CreateOrchidAsync(request);
                    return result.IsError
                        ? Results.BadRequest()
                        : Results.Ok(result);
                })
            ;
    }
}