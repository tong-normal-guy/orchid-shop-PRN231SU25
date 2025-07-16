using AutoMapper;
using OrchidsShop.BLL.Commons.Results;
using OrchidsShop.BLL.Commons.Paginations;
using OrchidsShop.BLL.DTOs.Orders.Requests;
using OrchidsShop.BLL.DTOs.Orders.Responses;
using OrchidsShop.DAL.Contexts;
using OrchidsShop.DAL.Entities;
using OrchidsShop.DAL.Enums;
using OrchidsShop.BLL.Commons;

namespace OrchidsShop.BLL.Services;

public class OrderService : IOrderService
{
    private readonly IUnitOfWork _unitOfWork;
    private readonly IMapper _mapper;

    public OrderService(IUnitOfWork unitOfWork, IMapper mapper)
    {
        _unitOfWork = unitOfWork;
        _mapper = mapper;
    }

    /// <summary>
    /// Queries orders based on the provided request parameters.
    /// </summary>
    /// <param name="request">Query parameters for filtering, pagination, and sorting.</param>
    /// <returns>Operation result containing list of order DTOs with pagination metadata.</returns>
    public async Task<OperationResult<List<QueryOrderResponse>>> QueryOrdersAsync(QueryOrderRequest request)
    {
        try
        {
            var repository = _unitOfWork.Repository<Order>();
            
            // Include related entities for complete data
            string includeProperties = "Account,Account.Role,OrderDetails,OrderDetails.Orchid,OrderDetails.Orchid.Category";
            
            // Get data with pagination and count
            var (orders, totalCount) = repository.GetWithCount(
                filter: request.GetExpressions(),
                orderBy: request.GetOrder(),
                includeProperties: includeProperties,
                pageIndex: request.PageNumber - 1, // Convert to 0-based index
                pageSize: request.PageSize
            );
            
            // Map to response DTOs
            var orderDtos = _mapper.Map<List<QueryOrderResponse>>(orders);
            
            // Create pagination metadata
            var pagination = new Pagination(
                pageIndex: request.PageNumber - 1, // 0-based index
                pageSize: request.PageSize,
                count: totalCount
            );
            
            // Create success result and set metadata
            var result = OperationResult<List<QueryOrderResponse>>.Success(
                payload: orderDtos,
                statusCode: StatusCode.Ok,
                message: "Orders retrieved successfully"
            );
            
            result.MetaData = pagination;
            return result;
        }
        catch (Exception ex)
        {
            return OperationResult<List<QueryOrderResponse>>.Failure(
                statusCode: StatusCode.ServerError,
                messages: new List<string> { $"Error querying orders: {ex.Message}" }
            );
        }
    }

    /// <summary>
    /// Creates a new order with PENDING status.
    /// </summary>
    /// <param name="request">Order creation request with order details.</param>
    /// <returns>Operation result indicating success or failure.</returns>
    public async Task<OperationResult<bool>> CreateOrderAsync(CommandOrderRequest request)
    {
        try
        {
            var orderRepository = _unitOfWork.Repository<Order>();
            var orderDetailRepository = _unitOfWork.Repository<OrderDetail>();

            // Create new order entity
            var orderId = Guid.NewGuid();
            var orderEntity = new Order
            {
                Id = orderId,
                AccountId = request.AccountId!.Value,
                OrderDate = DateOnly.FromDateTime(DateTime.Now),
                Status = request.Status ?? EnumOrderStatus.Pending.ToString(),
                TotalAmount = 0 // Will be calculated from order details
            };
            
            // Calculate total amount and create order details
            decimal totalAmount = 0;
            var orderDetails = new List<OrderDetail>();
            
            if (request.OrderDetails != null && request.OrderDetails.Any())
            {
                foreach (var detailRequest in request.OrderDetails)
                {
                    var detailId = Guid.NewGuid();
                    var orderDetail = new OrderDetail
                    {
                        Id = detailId,
                        OrderId = orderEntity.Id,
                        OrchidId = detailRequest.OrchidId!.Value,
                        Quantity = detailRequest.Quantity!.Value,
                        Price = detailRequest.Price!.Value
                    };
                    
                    totalAmount += orderDetail.Price * orderDetail.Quantity;
                    orderDetails.Add(orderDetail);
                }
            }
            
            orderEntity.TotalAmount = totalAmount;
            
            // Add entities to repositories
            await orderRepository.AddAsync(orderEntity, false);
            
            foreach (var orderDetail in orderDetails)
            {
                await orderDetailRepository.AddAsync(orderDetail, false);
            }
            
            // Save changes
            var savedChanges = await _unitOfWork.SaveManualChangesAsync();
            
            if (savedChanges > 0)
            {
                return OperationResult<bool>.Success(
                    payload: true,
                    statusCode: StatusCode.Created,
                    message: "Order created successfully"
                );
            }
            else
            {
                return OperationResult<bool>.Failure(
                    statusCode: StatusCode.BadRequest,
                    messages: new List<string> { "Failed to create order" }
                );
            }
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Failure(
                statusCode: StatusCode.ServerError,
                messages: new List<string> { $"Error creating order: {ex.Message}" }
            );
        }
    }

    /// <summary>
    /// Updates an existing order (mainly status updates).
    /// </summary>
    /// <param name="request">Order update request with new data.</param>
    /// <returns>Operation result indicating success or failure.</returns>
    public async Task<OperationResult<bool>> UpdateOrderAsync(CommandOrderRequest request)
    {
        try
        {
            var repository = _unitOfWork.Repository<Order>();
            
            // Find existing order
            var existingOrder = await repository.FindAsync(request.Id!.Value);
            if (existingOrder == null)
            {
                return OperationResult<bool>.Failure(
                    statusCode: StatusCode.NotFound,
                    messages: new List<string> { "Order not found" }
                );
            }
            
            // Update order properties using ReflectionHelper
            ReflectionHepler.UpdateProperties(request, existingOrder);
            
            // Update the entity
            repository.Update(existingOrder);
            
            // Save changes
            var savedChanges = await _unitOfWork.SaveManualChangesAsync();
            
            if (savedChanges > 0)
            {
                return OperationResult<bool>.Success(
                    payload: true,
                    statusCode: StatusCode.Ok,
                    message: "Order updated successfully"
                );
            }
            else
            {
                return OperationResult<bool>.Failure(
                    statusCode: StatusCode.BadRequest,
                    messages: new List<string> { "Failed to update order" }
                );
            }
        }
        catch (Exception ex)
        {
            return OperationResult<bool>.Failure(
                statusCode: StatusCode.ServerError,
                messages: new List<string> { $"Error updating order: {ex.Message}" }
            );
        }
    }
}
