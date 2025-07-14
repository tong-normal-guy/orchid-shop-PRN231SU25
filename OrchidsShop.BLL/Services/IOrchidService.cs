using System;
using OrchidsShop.BLL.Commons.Results;
using OrchidsShop.BLL.DTOs.Orchids.Requests;
using OrchidsShop.BLL.DTOs.Orchids.Responses;

namespace OrchidsShop.BLL.Services;

public interface IOrchidService
{
    Task<OperationResult<List<QueryOrchidResponse>>> QueryOrchidsAsync(QueryOrchidRequest request);
    Task<OperationResult<bool>> CreateOrchidAsync(CommandOrchidRequest request);
}
