using System;
using AutoMapper;
using OrchidsShop.BLL.Commons.Paginations;
using OrchidsShop.BLL.Commons.Results;
using OrchidsShop.BLL.DTOs.Orchids.Requests;
using OrchidsShop.BLL.DTOs.Orchids.Responses;
using OrchidsShop.DAL.Contexts;
using OrchidsShop.DAL.Entities;
using OrchidsShop.DAL.Repos;
using OrchidsShop.BLL.Commons;

namespace OrchidsShop.BLL.Services;

public class OrchidService : IOrchidService
{
    private readonly IUnitOfWork _uow;
    private readonly IMapper _mapper;

    public OrchidService(IUnitOfWork uow, IMapper mapper)
    {
        _uow = uow;
        _mapper = mapper;
    }

    public async Task<OperationResult<List<QueryOrchidResponse>>> QueryOrchidsAsync(QueryOrchidRequest request)
    {
        var result = new OperationResult<List<QueryOrchidResponse>>();
        var pagin = new Pagination();
        pagin.PageSize = request.PageSize;
        
        var query = _uow.OrchidRepository
            .GetWithCount(
                filter: request.GetExpressions(),
                pageIndex: request.PageNumber,
                pageSize: request.PageSize,
                orderBy: request.GetOrder(),
                includeProperties: nameof(Category) /*"Category"*/);
        
        if (query.Data == null || !query.Data.Any())
        {
            result.AddResponseStatusCode(
                StatusCode.Ok,
                "No orchids found",
            []
                );
            return result;
        }

        pagin.TotalItemsCount = query.Count;

        result.AddResponseStatusCode(
            StatusCode.Ok,
            "Orchids retrieved successfully",
            _mapper.Map<List<QueryOrchidResponse>>(query.Data),
            pagin
            );
        return result;
    }

    public async Task<OperationResult<bool>> CreateOrchidAsync(CommandOrchidRequest request)
    {
        var result = new OperationResult<bool>();
        
        if (await _uow.Repository<Category>().FindAsync(request.CategoryId) == null)
        {
            result.AddError(StatusCode.BadRequest, "No categories found.");
            return result;
        }
        
        var existingOrchid = await _uow.OrchidRepository
            .SingleOrDefaultAsync(o => o.Name == request.Name);
        
        if (existingOrchid != null)
        {
            result.AddError(StatusCode.BadRequest, "Orchid with the same name already exists.");
            return result;
        }

        var orchid = _mapper.Map<Orchid>(request);
        var id = Guid.NewGuid();
        orchid.Id = id;

        await _uow.OrchidRepository.AddAsync(orchid);
        await _uow.SaveChangesAsync();

        result.AddResponseStatusCode(
            StatusCode.Created,
            "Orchid created successfully",
            true
            );
        
        return result;
    }

    /// <summary>
    /// Updates an existing orchid with new information.
    /// </summary>
    /// <param name="request">Command request containing orchid update data.</param>
    /// <returns>OperationResult&lt;bool&gt; indicating success or failure.</returns>
    public async Task<OperationResult<bool>> UpdateOrchidAsync(CommandOrchidRequest request)
    {
        var result = new OperationResult<bool>();
        
        if (request.Id == null)
        {
            result.AddError(StatusCode.BadRequest, "Orchid ID is required for update.");
            return result;
        }

        var existingOrchid = await _uow.OrchidRepository
            .SingleOrDefaultAsync(o => o.Id == request.Id);
        
        if (existingOrchid == null)
        {
            result.AddError(StatusCode.NotFound, "Orchid not found.");
            return result;
        }

        // Check if category exists if CategoryId is provided
        if (request.CategoryId != null)
        {
            var categoryExists = await _uow.Repository<Category>()
                .SingleOrDefaultAsync(c => c.Id == request.CategoryId);
            if (categoryExists == null)
            {
                result.AddError(StatusCode.BadRequest, "Category not found.");
                return result;
            }
        }

        // Check for duplicate name if name is being updated
        if (!string.IsNullOrEmpty(request.Name) && request.Name != existingOrchid.Name)
        {
            var duplicateOrchid = await _uow.OrchidRepository
                .SingleOrDefaultAsync(o => o.Name == request.Name && o.Id != request.Id);
            
            if (duplicateOrchid != null)
            {
                result.AddError(StatusCode.BadRequest, "Orchid with the same name already exists.");
                return result;
            }
        }

        // Use ReflectionHelper to update properties
        ReflectionHepler.UpdateProperties(request, existingOrchid);
        
        _uow.OrchidRepository.Update(existingOrchid);
        var saveResult = await _uow.SaveManualChangesAsync();

        if (saveResult > 0)
        {
            result.AddResponseStatusCode(
                StatusCode.Ok,
                "Orchid updated successfully",
                true
            );
        }
        else
        {
            result.AddError(StatusCode.ServerError, "Failed to update orchid.");
        }
        
        return result;
    }

    /// <summary>
    /// Deletes an orchid by its ID.
    /// </summary>
    /// <param name="id">The ID of the orchid to delete.</param>
    /// <returns>OperationResult&lt;bool&gt; indicating success or failure.</returns>
    public async Task<OperationResult<bool>> DeleteOrchidAsync(Guid id)
    {
        var result = new OperationResult<bool>();
        
        var existingOrchid = await _uow.OrchidRepository
            .SingleOrDefaultAsync(o => o.Id == id);
        
        if (existingOrchid == null)
        {
            result.AddError(StatusCode.NotFound, "Orchid not found.");
            return result;
        }

        await _uow.OrchidRepository.RemoveAsync(existingOrchid, false);
        var saveResult = await _uow.SaveManualChangesAsync();

        if (saveResult > 0)
        {
            result.AddResponseStatusCode(
                StatusCode.Ok,
                "Orchid deleted successfully",
                true
            );
        }
        else
        {
            result.AddError(StatusCode.ServerError, "Failed to delete orchid.");
        }
        
        return result;
    }
}
