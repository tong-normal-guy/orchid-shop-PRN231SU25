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
    
        /// <summary>
        /// Tạo mới một hoa lan.
        /// </summary>
        /// <param name="request">Thông tin hoa lan cần tạo.</param>
        /// <param name="service">Service xử lý nghiệp vụ hoa lan.</param>
        /// <returns>Kết quả tạo hoa lan.</returns>
        app.MapPost(
                Route,
                async (CommandOrchidRequest request, IOrchidService service) =>
                {
                    var result = await service.CreateOrchidAsync(request);
                    return result.IsError
                        ? Results.BadRequest()
                        : Results.Ok(result);
                })
            .WithDisplayName("Create Orchid")
            .WithDescription("Creates a new orchid.")
            .WithTags("Orchids")
            ;

        /// <summary>
        /// Cập nhật thông tin hoa lan.
        /// </summary>
        /// <param name="request">Thông tin hoa lan cần cập nhật.</param>
        /// <param name="service">Service xử lý nghiệp vụ hoa lan.</param>
        /// <returns>Kết quả cập nhật hoa lan.</returns>
        app.MapPut(
                Route,
                async (CommandOrchidRequest request, IOrchidService service) =>
                {
                    var result = await service.UpdateOrchidAsync(request);
                    return result.IsError
                        ? Results.BadRequest(result)
                        : Results.Ok(result);
                })
            .WithDisplayName("Update Orchid")
            .WithDescription("Updates an existing orchid.")
            .WithTags("Orchids")
            ;

        /// <summary>
        /// Xóa hoa lan theo ID.
        /// </summary>
        /// <param name="id">ID của hoa lan cần xóa.</param>
        /// <param name="service">Service xử lý nghiệp vụ hoa lan.</param>
        /// <returns>Kết quả xóa hoa lan.</returns>
        app.MapDelete(
                $"{Route}/{{id:guid}}",
                async (Guid id, IOrchidService service) =>
                {
                    var result = await service.DeleteOrchidAsync(id);
                    return result.IsError
                        ? Results.BadRequest(result)
                        : Results.Ok(result);
                })
            .WithDisplayName("Delete Orchid")
            .WithDescription("Deletes an orchid by ID.")
            .WithTags("Orchids")
            ;
    }
}