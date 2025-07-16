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
        /// Tạo mới một hoa lan với thông tin đầy đủ.
        /// </summary>
        /// <param name="request">Thông tin hoa lan cần tạo bao gồm: name, description, url, price, isNatural, categoryId.</param>
        /// <param name="service">Service xử lý nghiệp vụ hoa lan.</param>
        /// <returns>Kết quả tạo hoa lan (OperationResult&lt;bool&gt;).</returns>
        app.MapPost(
                Route,
                async (CommandOrchidRequest request, IOrchidService service) =>
                {
                    var result = await service.CreateOrchidAsync(request);
                    if (result.IsError)
                    {
                        return Results.BadRequest(new
                        {
                            success = false,
                            message = result.Message,
                            errors = result.Errors
                        });
                    }
                    return Results.Ok(new
                    {
                        success = true,
                        message = result.Message
                    });
                })
            .WithDisplayName("Create Orchid")
            .WithDescription("Tạo sản phẩm hoa lan mới với thông tin đầy đủ. Trường bắt buộc: name, price, isNatural, categoryId. " +
                           "Các trường tùy chọn: description, url. Sử dụng ReflectionHelper để mapping dữ liệu.")
            .WithTags("Orchids")
            ;

        /// <summary>
        /// Cập nhật thông tin hoa lan sử dụng ReflectionHelper.
        /// </summary>
        /// <param name="request">Thông tin hoa lan cần cập nhật, bắt buộc có ID. Các trường khác sẽ được cập nhật nếu có giá trị.</param>
        /// <param name="service">Service xử lý nghiệp vụ hoa lan.</param>
        /// <returns>Kết quả cập nhật (OperationResult&lt;bool&gt;).</returns>
        app.MapPut(
                Route,
                async (CommandOrchidRequest request, IOrchidService service) =>
                {
                    var result = await service.UpdateOrchidAsync(request);
                    if (result.IsError)
                    {
                        return Results.BadRequest(new
                        {
                            success = false,
                            message = result.Message,
                            errors = result.Errors
                        });
                    }
                    return Results.Ok(new
                    {
                        success = true,
                        message = result.Message
                    });
                })
            .WithDisplayName("Update Orchid")
            .WithDescription("Cập nhật thông tin hoa lan hiện có. Bắt buộc có ID trong request body. " +
                           "Chỉ cập nhật các trường có giá trị (không null). Sử dụng ReflectionHelper để mapping linh hoạt.")
            .WithTags("Orchids")
            ;

        /// <summary>
        /// Xóa hoa lan theo ID với kiểm tra ràng buộc dữ liệu.
        /// </summary>
        /// <param name="id">ID của hoa lan cần xóa (Guid format).</param>
        /// <param name="service">Service xử lý nghiệp vụ hoa lan.</param>
        /// <returns>Kết quả xóa (OperationResult&lt;bool&gt;).</returns>
        app.MapDelete(
                $"{Route}/{{id:guid}}",
                async (Guid id, IOrchidService service) =>
                {
                    var result = await service.DeleteOrchidAsync(id);
                    if (result.IsError)
                    {
                        return Results.BadRequest(new
                        {
                            success = false,
                            message = result.Message,
                            errors = result.Errors
                        });
                    }
                    return Results.Ok(new
                    {
                        success = true,
                        message = result.Message
                    });
                })
            .WithDisplayName("Delete Orchid")
            .WithDescription("Xóa hoa lan theo ID. Kiểm tra ràng buộc dữ liệu trước khi xóa (ví dụ: đơn hàng liên quan). " +
                           "Trả về lỗi nếu không thể xóa do vi phạm tính toàn vẹn dữ liệu.")
            .WithTags("Orchids")
            ;
    }
}