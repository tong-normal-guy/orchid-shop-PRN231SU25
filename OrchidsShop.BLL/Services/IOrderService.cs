using System;
using OrchidsShop.BLL.Commons.Results;
using OrchidsShop.BLL.DTOs.Orders.Requests;
using OrchidsShop.BLL.DTOs.Orders.Responses;

namespace OrchidsShop.BLL.Services;

public interface IOrderService
{
    /// <summary>
    /// Queries orders based on the provided request parameters.
    /// </summary>
    /// <param name="request">Query parameters for filtering, pagination, and sorting.</param>
    /// <returns>Operation result containing list of order DTOs.</returns>
    Task<OperationResult<List<QueryOrderResponse>>> QueryOrdersAsync(QueryOrderRequest request);

    /// <summary>
    /// Creates a new order with PENDING status.
    /// </summary>
    /// <param name="request">Order creation request with order details.</param>
    /// <returns>Operation result indicating success or failure.</returns>
    Task<OperationResult<bool>> CreateOrderAsync(CommandOrderRequest request);

    /// <summary>
    /// Updates an existing order (mainly status updates).
    /// </summary>
    /// <param name="request">Order update request with new data.</param>
    /// <returns>Operation result indicating success or failure.</returns>
    Task<OperationResult<bool>> UpdateOrderAsync(CommandOrderRequest request);
}
