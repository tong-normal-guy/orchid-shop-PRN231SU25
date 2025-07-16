using Carter;
using OrchidsShop.BLL.DTOs.Categories.Requests;
using OrchidsShop.BLL.DTOs.Orchids.Requests;
using OrchidsShop.BLL.Services;

namespace OrchidsShop.API.Endpoints;

public class OrchidCategoryEndpoint : ICarterModule
{
    private const string Route = "/api/orchid-categories";
    private const string Tags = "Orchid Categories";
    // private readonly OrchidCategoryService _service;
    //
    // public OrchidCategoryEndpoint(OrchidCategoryService service)
    // {
    //     _service = service;
    // }

    public void AddRoutes(IEndpointRouteBuilder app)
    {
        // GET all categories with pagination, filtering, and sorting
        // This handles all query operations including by ID, by name, search, etc.
        // app.MapGet(
        //         Route,
        //         async ([AsParameters]QueryCategoryRequest request, OrchidCategoryService service) =>
        //         {
        //             var result = await service.QueryOrchidCategoriesAsync(request);
        //             return result.IsError
        //                 ? Results.BadRequest(new { 
        //                     message = result.Message, 
        //                     errors = result.Errors 
        //                 })
        //                 : Results.Ok(new { 
        //                     message = result.Message, 
        //                     data = result.Payload,
        //                     pagination = result.MetaData 
        //                 });
        //         })
        // .WithDisplayName("Query Orchid Categories")
        // .WithDescription("Retrieves orchid categories with pagination, filtering, and sorting. Supports search by name, filter by IDs, and all other query operations.")
        ;

        // POST - Create multiple categories with validation and bulk processing
        app.MapPost(
                Route,
                async (List<CommandCategoryRequest> request, OrchidCategoryService service) =>
                {
                    var result = await service.CreateOrchidCategoryAsync(request);
                    return result.IsError
                        ? Results.BadRequest(new
                        {
                            message = result.Message,
                            errors = result.Errors
                        })
                        : Results.Created("", new
                        {
                            message = result.Message,
                            success = result.Payload
                        });
                })
            .WithDisplayName("Create Orchid Categories")
            .WithDescription("Tạo nhiều danh mục hoa lan từ danh sách. Tự động kiểm tra trùng lặp, lọc bỏ dữ liệu không hợp lệ và sử dụng bulk insertion để tối ưu hiệu suất. " +
                           "Chỉ tạo các danh mục có tên hợp lệ (2-255 ký tự, không trùng với danh mục hiện có).")
            .WithName("Create Orchid Categories")
            .WithTags(Tags)
            ;

        // PUT - Full update category using ReflectionHelper
        app.MapPut(
                Route + "/{id:guid}",
                async (Guid id, CommandCategoryRequest request, OrchidCategoryService service) =>
                {
                    var result = await service.UpdateCategoryAsync(id, request);
                    return result.IsError
                        ? Results.BadRequest(new
                        {
                            message = result.Message,
                            errors = result.Errors
                        })
                        : Results.Ok(new
                        {
                            message = result.Message,
                            success = result.Payload
                        });
                })
            .WithDisplayName("Update Orchid Category")
            .WithDescription("Cập nhật danh mục hoa lan hiện có với validation toàn diện. Sử dụng ReflectionHelper để cập nhật linh hoạt các thuộc tính. " +
                           "Kiểm tra tính duy nhất của tên danh mục và validation dữ liệu đầu vào.")
            .WithTags(Tags)
            ;

        // PATCH - Partial update category using ReflectionHelper
        app.MapPatch(
                Route + "/{id:guid}",
                async (Guid id, CommandCategoryRequest request, OrchidCategoryService service) =>
                {
                    var result = await service.PartialUpdateCategoryAsync(id, request);
                    return result.IsError
                        ? Results.BadRequest(new
                        {
                            message = result.Message,
                            errors = result.Errors
                        })
                        : Results.Ok(new
                        {
                            message = result.Message,
                            success = result.Payload
                        });
                })
            .WithDisplayName("Partial Update Orchid Category")
            .WithDescription("Cập nhật một phần danh mục hoa lan sử dụng ReflectionHelper. Chỉ cập nhật các thuộc tính được cung cấp (không null/empty). " +
                           "Tối ưu cho việc cập nhật từng trường cụ thể mà không ảnh hưởng đến các trường khác.")
            .WithTags(Tags)
            ;

        // PUT - Bulk update multiple categories using ReflectionHelper
        // app.MapPut(
        //         Route + "/bulk",
        //         async (BulkUpdateCategoryRequest request) =>
        //         {
        //             var result = await _service.BulkUpdateCategoriesAsync(request.CategoryIds, request.UpdateData);
        //             return result.IsError
        //                 ? Results.BadRequest(new { 
        //                     message = result.Message, 
        //                     errors = result.Errors 
        //                 })
        //                 : Results.Ok(new { 
        //                     message = result.Message, 
        //                     success = result.Payload 
        //                 });
        //         })
        //     .WithDisplayName("Bulk Update Orchid Categories")
        //     .WithDescription("Updates multiple orchid categories at once using ReflectionHelper for efficient bulk operations.")
        //     ;

        // DELETE - Delete category with referential integrity checks
        app.MapDelete(
                Route + "/{id:guid}",
                async (Guid id, OrchidCategoryService service) =>
                {
                    var result = await service.DeleteCategoryAsync(id);
                    return result.IsError
                        ? Results.BadRequest(new
                        {
                            message = result.Message,
                            errors = result.Errors
                        })
                        : Results.Ok(new
                        {
                            message = result.Message,
                            success = result.Payload
                        });
                })
            .WithDisplayName("Delete Orchid Category")
            .WithDescription("Xóa danh mục hoa lan sau khi kiểm tra các hoa lan liên quan để duy trì tính toàn vẹn dữ liệu. " +
                           "Trả về lỗi nếu có hoa lan thuộc danh mục này để tránh mất dữ liệu quan trọng.")
            .WithTags(Tags)
            ;
    }
}