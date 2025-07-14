using AutoMapper;
using Microsoft.EntityFrameworkCore;
using OrchidsShop.BLL.Commons;
using OrchidsShop.BLL.Commons.Paginations;
using OrchidsShop.BLL.Commons.Results;
using OrchidsShop.BLL.DTOs.Categories.Requests;
using OrchidsShop.BLL.DTOs.Categories.Responses;
using OrchidsShop.DAL.Contexts;
using OrchidsShop.DAL.Entities;

namespace OrchidsShop.BLL.Services;

public class OrchidCategoryService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public OrchidCategoryService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }
    
    /// <summary>
    /// Query orchid categories with pagination, filtering, and sorting
    /// This handles all GET operations including by ID, by name, etc.
    /// </summary>
    public async Task<OperationResult<List<QueryCategoryResponse>>> QueryOrchidCategoriesAsync(QueryCategoryRequest request)
    {
        var result = new OperationResult<List<QueryCategoryResponse>>();
        var pagin = new Pagination
        {
            PageSize = request.PageSize
        };

        // Use the more efficient GetWithCount method
        var query = _uow.Repository<Category>()
            .GetWithCount(
                filter: request.GetExpressions(),
                pageIndex: request.PageNumber,
                pageSize: request.PageSize,
                orderBy: request.GetOrder());

        if (query.Data == null || !query.Data.Any())
        {
            result.AddResponseStatusCode(
                StatusCode.Ok,
                "No orchid categories found",
                new List<QueryCategoryResponse>()
            );
            return result;
        }

        pagin.TotalItemsCount = query.Count;

        result.AddResponseStatusCode(
            StatusCode.Ok,
            "Orchid categories retrieved successfully",
            _mapper.Map<List<QueryCategoryResponse>>(query.Data),
            pagin
        );
        return result;
    }
    
    /// <summary>
    /// Create multiple orchid categories with validation and filtering
    /// Handles list processing, duplicate checking, and bulk insertion
    /// </summary>
    public async Task<OperationResult<bool>> CreateOrchidCategoryAsync(List<CommandCategoryRequest> request)
    {
        var result = new OperationResult<bool>();

        // Filter out null or empty objects
        var validRequests = request?
            .Where(r => r != null && !string.IsNullOrWhiteSpace(r.Name))
            .ToList();

        if (validRequests == null || !validRequests.Any())
        {
            result.AddResponseStatusCode(
                StatusCode.BadRequest,
                "No valid category data provided",
                false
            );
            return result;
        }

        // Get category names to check (case-insensitive)
        var categoryNamesToCheck = validRequests.Select(r => r.Name!.ToLower()).ToList();

        // Use Where() method to get existing categories more efficiently
        var existingCategories = await _uow.Repository<Category>()
            .Where(c => categoryNamesToCheck.Contains(c.Name.ToLower()))
            .Select(c => c.Name.ToLower())
            .ToListAsync();

        // Filter out categories that already exist
        var newCategories = validRequests
            .Where(r => !existingCategories.Contains(r.Name!.ToLower()))
            .ToList();

        if (!newCategories.Any())
        {
            result.AddResponseStatusCode(
                StatusCode.BadRequest,
                "All provided categories already exist",
                false
            );
            return result;
        }

        // Create new categories with generated IDs
        var categoriesToAdd = newCategories.Select(request =>
        {
            var category = _mapper.Map<Category>(request);
            category.Id = Guid.NewGuid();
            return category;
        }).ToList();

        // Use AddRangeAsync with saveChanges = false for better control
        await _uow.Repository<Category>().AddRangeAsync(categoriesToAdd, false);
        
        // Use UnitOfWork's SaveChangesAsync for better transaction control
        await _uow.SaveChangesAsync();

        var createdCount = categoriesToAdd.Count;
        var duplicateCount = validRequests.Count - createdCount;
        
        string message = $"Successfully created {createdCount} categories";
        if (duplicateCount > 0)
        {
            message += $". {duplicateCount} categories were skipped (already exist)";
        }

        result.AddResponseStatusCode(
            StatusCode.Created,
            message,
            true
        );
        return result;
    }

    /// <summary>
    /// Update category with comprehensive validation and ReflectionHelper for flexible updates
    /// Handles existence checking, duplicate name validation, and referential integrity
    /// </summary>
    public async Task<OperationResult<bool>> UpdateCategoryAsync(Guid id, CommandCategoryRequest request)
    {
        var result = new OperationResult<bool>();
        
        // Validate input
        if (string.IsNullOrWhiteSpace(request.Name))
        {
            result.AddResponseStatusCode(
                StatusCode.BadRequest,
                "Category name is required and cannot be empty",
                false
            );
            return result;
        }

        // Trim and validate name length
        var trimmedName = request.Name.Trim();
        if (trimmedName.Length < 2)
        {
            result.AddResponseStatusCode(
                StatusCode.BadRequest,
                "Category name must be at least 2 characters long",
                false
            );
            return result;
        }

        if (trimmedName.Length > 255)
        {
            result.AddResponseStatusCode(
                StatusCode.BadRequest,
                "Category name cannot exceed 255 characters",
                false
            );
            return result;
        }

        // Use FindAsync to get the category efficiently
        var category = await _uow.Repository<Category>().FindAsync(id);
        
        if (category == null)
        {
            result.AddResponseStatusCode(
                StatusCode.NotFound,
                "Category not found",
                false
            );
            return result;
        }

        // Store old values for audit trail
        var oldName = category.Name;

        // Create a temporary request with trimmed name for consistent processing
        var processedRequest = new CommandCategoryRequest { Name = trimmedName };

        // Check if the name is actually changing using reflection comparison
        var hasChanges = !string.Equals(category.Name, processedRequest.Name, StringComparison.OrdinalIgnoreCase);

        if (!hasChanges)
        {
            result.AddResponseStatusCode(
                StatusCode.Ok,
                "Category is already up to date - no changes detected",
                true
            );
            return result;
        }

        // Check if another category with the same name exists (case-insensitive)
        var existingCategory = await _uow.Repository<Category>()
            .FirstOrDefaultAsync(c => c.Name.ToLower() == trimmedName.ToLower() && c.Id != id);
        
        if (existingCategory != null)
        {
            result.AddResponseStatusCode(
                StatusCode.BadRequest,
                $"A category with the name '{trimmedName}' already exists",
                false
            );
            return result;
        }

        // Check if category has associated orchids (optional - depending on business rules)
        var hasOrchids = await _uow.Repository<Orchid>()
            .FirstOrDefaultAsync(o => o.CategoryId == id);
        
        if (hasOrchids != null)
        {
            // Log that category has orchids but still allow update
            // You might want to add logging here if needed
        }

        try
        {
            // Use ReflectionHelper to update properties (excluding Id and navigation properties)
            ReflectionHepler.UpdateProperties(processedRequest, category, new List<string> { "Id", "Orchids" });
            
            // Use UpdateAsync with saveChanges = false for better transaction control
            await _uow.Repository<Category>().UpdateAsync(category, false);
            await _uow.SaveChangesAsync();

            result.AddResponseStatusCode(
                StatusCode.Ok,
                $"Category updated successfully from '{oldName}' to '{category.Name}'",
                true
            );
        }
        catch (Exception ex)
        {
            // Handle any database errors
            result.AddResponseStatusCode(
                StatusCode.ServerError,
                "An error occurred while updating the category. Please try again.",
                false
            );
            // You might want to log the exception here
        }

        return result;
    }

    /// <summary>
    /// Partial update category using ReflectionHelper for selective property updates
    /// Only updates properties that are provided (not null/empty)
    /// </summary>
    public async Task<OperationResult<bool>> PartialUpdateCategoryAsync(Guid id, CommandCategoryRequest request)
    {
        var result = new OperationResult<bool>();

        // Use FindAsync to get the category efficiently
        var category = await _uow.Repository<Category>().FindAsync(id);
        
        if (category == null)
        {
            result.AddResponseStatusCode(
                StatusCode.NotFound,
                "Category not found",
                false
            );
            return result;
        }

        // Store old values for audit trail
        var oldName = category.Name;
        var changesList = new List<string>();

        // Validate and process name if provided
        if (!string.IsNullOrWhiteSpace(request.Name))
        {
            var trimmedName = request.Name.Trim();
            
            // Validate name length
            if (trimmedName.Length < 2)
            {
                result.AddResponseStatusCode(
                    StatusCode.BadRequest,
                    "Category name must be at least 2 characters long",
                    false
                );
                return result;
            }

            if (trimmedName.Length > 255)
            {
                result.AddResponseStatusCode(
                    StatusCode.BadRequest,
                    "Category name cannot exceed 255 characters",
                    false
                );
                return result;
            }

            // Check for duplicates only if name is changing
            if (!string.Equals(category.Name, trimmedName, StringComparison.OrdinalIgnoreCase))
            {
                var existingCategory = await _uow.Repository<Category>()
                    .FirstOrDefaultAsync(c => c.Name.ToLower() == trimmedName.ToLower() && c.Id != id);
                
                if (existingCategory != null)
                {
                    result.AddResponseStatusCode(
                        StatusCode.BadRequest,
                        $"A category with the name '{trimmedName}' already exists",
                        false
                    );
                    return result;
                }

                changesList.Add($"Name: '{oldName}' → '{trimmedName}'");
                request.Name = trimmedName; // Update request with processed name
            }
            else
            {
                request.Name = null; // No change needed, set to null to skip update
            }
        }

        // Check if there are any actual changes to make
        if (!changesList.Any())
        {
            result.AddResponseStatusCode(
                StatusCode.Ok,
                "Category is already up to date - no changes detected",
                true
            );
            return result;
        }

        try
        {
            // Use ReflectionHelper to update only non-null properties
            ReflectionHepler.UpdateProperties(request, category, new List<string> { "Id", "Orchids" });
            
            // Use UpdateAsync with saveChanges = false for better transaction control
            await _uow.Repository<Category>().UpdateAsync(category, false);
            await _uow.SaveChangesAsync();

            var changesDescription = string.Join(", ", changesList);
            result.AddResponseStatusCode(
                StatusCode.Ok,
                $"Category partially updated successfully. Changes: {changesDescription}",
                true
            );
        }
        catch (Exception ex)
        {
            // Handle any database errors
            result.AddResponseStatusCode(
                StatusCode.ServerError,
                "An error occurred while updating the category. Please try again.",
                false
            );
            // You might want to log the exception here
        }

        return result;
    }

    /// <summary>
    /// Bulk update multiple categories using ReflectionHelper
    /// Applies the same updates to multiple categories efficiently
    /// </summary>
    // public async Task<OperationResult<bool>> BulkUpdateCategoriesAsync(List<Guid> categoryIds, CommandCategoryRequest updateRequest)
    // {
    //     var result = new OperationResult<bool>();

    //     if (categoryIds == null || !categoryIds.Any())
    //     {
    //         result.AddResponseStatusCode(
    //             StatusCode.BadRequest,
    //             "No category IDs provided for bulk update",
    //             false
    //         );
    //         return result;
    //     }

    //     // Validate update request
    //     if (string.IsNullOrWhiteSpace(updateRequest.Name))
    //     {
    //         result.AddResponseStatusCode(
    //             StatusCode.BadRequest,
    //             "Update data is required",
    //             false
    //         );
    //         return result;
    //     }

    //     // Get all categories to update
    //     var categories = await _uow.Repository<Category>()
    //         .Where(c => categoryIds.Contains(c.Id))
    //         .ToListAsync();

    //     if (!categories.Any())
    //     {
    //         result.AddResponseStatusCode(
    //             StatusCode.NotFound,
    //             "No categories found with the provided IDs",
    //             false
    //         );
    //         return result;
    //     }

    //     var notFoundIds = categoryIds.Except(categories.Select(c => c.Id)).ToList();
    //     var updatedCount = 0;
    //     var skippedCount = 0;
    //     var errors = new List<string>();

    //     try
    //     {
    //         foreach (var category in categories)
    //         {
    //             try
    //             {
    //                 // Store old values for audit
    //                 var oldName = category.Name;

    //                 // Use ReflectionHelper to apply updates
    //                 ReflectionHepler.UpdateProperties(updateRequest, category, new List<string> { "Id", "Orchids" });

    //                 // Check if there were actual changes
    //                 if (!string.Equals(oldName, category.Name, StringComparison.OrdinalIgnoreCase))
    //                 {
    //                     updatedCount++;
    //                 }
    //                 else
    //                 {
    //                     skippedCount++;
    //                 }
    //             }
    //             catch (Exception ex)
    //             {
    //                 errors.Add($"Failed to update category {category.Id}: {ex.Message}");
    //                 skippedCount++;
    //             }
    //         }

    //         // Save all changes at once for better performance
    //         await _uow.SaveChangesAsync();

    //         var message = $"Bulk update completed. Updated: {updatedCount}, Skipped: {skippedCount}";
    //         if (notFoundIds.Any())
    //         {
    //             message += $", Not found: {notFoundIds.Count}";
    //         }
    //         if (errors.Any())
    //         {
    //             message += $", Errors: {errors.Count}";
    //         }

    //         result.AddResponseStatusCode(
    //             errors.Any() ? StatusCode.BadRequest : StatusCode.Ok,
    //             message,
    //             updatedCount > 0
    //         );
    //     }
    //     catch (Exception ex)
    //     {
    //         result.AddResponseStatusCode(
    //             StatusCode.ServerError,
    //             "An error occurred during bulk update. Please try again.",
    //             false
    //         );
    //     }

    //     return result;
    // }

    /// <summary>
    /// Delete category with comprehensive validation and referential integrity checks
    /// </summary>
    public async Task<OperationResult<bool>> DeleteCategoryAsync(Guid id)
    {
        var result = new OperationResult<bool>();
        
        // Use FindAsync to get the category
        var category = await _uow.Repository<Category>().FindAsync(id);
        
        if (category == null)
        {
            result.AddResponseStatusCode(
                StatusCode.NotFound,
                "Category not found",
                false
            );
            return result;
        }

        // Check if category has associated orchids
        var associatedOrchids = await _uow.Repository<Orchid>()
            .Where(o => o.CategoryId == id)
            .CountAsync();
        
        if (associatedOrchids > 0)
        {
            result.AddResponseStatusCode(
                StatusCode.BadRequest,
                $"Cannot delete category '{category.Name}' because it has {associatedOrchids} associated orchid(s). Please reassign or delete the orchids first.",
                false
            );
            return result;
        }

        try
        {
            // Store name for response message
            var categoryName = category.Name;

            // Use RemoveAsync with saveChanges = false for better transaction control
            await _uow.Repository<Category>().RemoveAsync(category, false);
            await _uow.SaveChangesAsync();

            result.AddResponseStatusCode(
                StatusCode.Ok,
                $"Category '{categoryName}' deleted successfully",
                true
            );
        }
        catch (Exception ex)
        {
            // Handle any database errors
            result.AddResponseStatusCode(
                StatusCode.ServerError,
                "An error occurred while deleting the category. Please try again.",
                false
            );
            // You might want to log the exception here
        }

        return result;
    }
}