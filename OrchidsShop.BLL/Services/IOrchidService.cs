using System;
using OrchidsShop.BLL.Commons.Results;
using OrchidsShop.BLL.DTOs.Orchids.Requests;
using OrchidsShop.BLL.DTOs.Orchids.Responses;

namespace OrchidsShop.BLL.Services;

public interface IOrchidService
{
    Task<OperationResult<List<QueryOrchidResponse>>> QueryOrchidsAsync(QueryOrchidRequest request);
    Task<OperationResult<bool>> CreateOrchidAsync(CommandOrchidRequest request);
    /// <summary>
    /// Updates an existing orchid with new information.
    /// </summary>
    /// <param name="request">Command request containing orchid update data.</param>
    /// <returns>OperationResult&lt;bool&gt; indicating success or failure.</returns>
    Task<OperationResult<bool>> UpdateOrchidAsync(CommandOrchidRequest request);
    /// <summary>
    /// Deletes an orchid by its ID.
    /// </summary>
    /// <param name="id">The ID of the orchid to delete.</param>
    /// <returns>OperationResult&lt;bool&gt; indicating success or failure.</returns>
    Task<OperationResult<bool>> DeleteOrchidAsync(Guid id);
}
