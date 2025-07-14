using Carter;
using OrchidsShop.BLL.DTOs.Categories.Requests;
using OrchidsShop.BLL.DTOs.Orchids.Requests;
using OrchidsShop.BLL.Services;

namespace OrchidsShop.API.Endpoints;

public class OrchidCategoryEndpoint : ICarterModule
{
    private const string Route = "/api/orchid-categories";
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
        app.MapGet(
                Route,
                async (QueryCategoryRequest request, OrchidCategoryService service) =>
                {
                    var result = await service.QueryOrchidCategoriesAsync(request);
                    return result.IsError
                        ? Results.BadRequest(new { 
                            message = result.Message, 
                            errors = result.Errors 
                        })
                        : Results.Ok(new { 
                            message = result.Message, 
                            data = result.Payload,
                            pagination = result.MetaData 
                        });
                })
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
                        ? Results.BadRequest(new { 
                            message = result.Message, 
                            errors = result.Errors 
                        })
                        : Results.Created("", new { 
                            message = result.Message, 
                            success = result.Payload 
                        });
                })
            .WithDisplayName("Create Orchid Categories")
            .WithDescription("Creates multiple orchid categories from a list. Validates for existing names, filters out null/empty objects, and uses bulk insertion for efficiency.")
            ;

        // PUT - Full update category using ReflectionHelper
        app.MapPut(
                Route + "/{id:guid}",
                async (Guid id, CommandCategoryRequest request, OrchidCategoryService service) =>
                {
                    var result = await service.UpdateCategoryAsync(id, request);
                    return result.IsError
                        ? Results.BadRequest(new { 
                            message = result.Message, 
                            errors = result.Errors 
                        })
                        : Results.Ok(new { 
                            message = result.Message, 
                            success = result.Payload 
                        });
                })
            // .WithDisplayName("Update Orchid Category")
            // .WithDescription("Updates an existing orchid category with comprehensive validation using ReflectionHelper for flexible property updates.")
            ;

        // PATCH - Partial update category using ReflectionHelper
        app.MapPatch(
                Route + "/{id:guid}",
                async (Guid id, CommandCategoryRequest request, OrchidCategoryService service) =>
                {
                    var result = await service.PartialUpdateCategoryAsync(id, request);
                    return result.IsError
                        ? Results.BadRequest(new { 
                            message = result.Message, 
                            errors = result.Errors 
                        })
                        : Results.Ok(new { 
                            message = result.Message, 
                            success = result.Payload 
                        });
                })
            // .WithDisplayName("Partial Update Orchid Category")
            // .WithDescription("Partially updates an orchid category using ReflectionHelper. Only updates properties that are provided (not null/empty).")
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
                        ? Results.BadRequest(new { 
                            message = result.Message, 
                            errors = result.Errors 
                        })
                        : Results.Ok(new { 
                            message = result.Message, 
                            success = result.Payload 
                        });
                })
            // .WithDisplayName("Delete Orchid Category")
            // .WithDescription("Deletes an orchid category after checking for associated orchids to maintain referential integrity.")
            ;
    }
}