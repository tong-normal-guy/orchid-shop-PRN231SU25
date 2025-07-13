using System;
using AutoMapper;
using OrchidsShop.BLL.Commons.Paginations;
using OrchidsShop.BLL.Commons.Results;
using OrchidsShop.BLL.DTOs.Orchids.Requests;
using OrchidsShop.BLL.DTOs.Orchids.Responses;
using OrchidsShop.DAL.Contexts;
using OrchidsShop.DAL.Entities;
using OrchidsShop.DAL.Repos;

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
}
