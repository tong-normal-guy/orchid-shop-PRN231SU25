using AutoMapper;
using OrchidsShop.BLL.Commons.Results;
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
    /// <returns>Operation result containing list of order DTOs.</returns>
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
                pageIndex: request.PageNumber,
                pageSize: request.PageSize
            );
            
            // Map to response DTOs
            var orderDtos = _mapper.Map<List<QueryOrderResponse>>(orders);
            
            // Create paginated result
            var result = new OperationResult<List<QueryOrderResponse>>
            {
                IsError = false,
                Payload = orderDtos
            };
            
            return result;
        }
        catch (Exception ex)
        {
            return new OperationResult<List<QueryOrderResponse>>
            {
                IsError = true,
                Message = $"Error querying orders: {ex.Message}",
                Payload = new List<QueryOrderResponse>()
            };
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
            var orderEntity = new Order
            {
                Id = Guid.NewGuid(),
                AccountId = request.AccountId!.Value,
                OrderDate = DateOnly.FromDateTime(DateTime.Now),
                Status = EnumOrderStatus.Pending.ToString(),
                TotalAmound = 0 // Will be calculated from order details
            };
            
            // Calculate total amount and create order details
            decimal totalAmount = 0;
            var orderDetails = new List<OrderDetail>();
            
            if (request.OrderDetails != null && request.OrderDetails.Any())
            {
                foreach (var detailRequest in request.OrderDetails)
                {
                    var orderDetail = new OrderDetail
                    {
                        Id = Guid.NewGuid(),
                        OrderId = orderEntity.Id,
                        OrchidId = detailRequest.OrchidId!.Value,
                        Quantity = detailRequest.Quantity!.Value,
                        Price = detailRequest.Price!.Value
                    };
                    
                    totalAmount += orderDetail.Price * orderDetail.Quantity;
                    orderDetails.Add(orderDetail);
                }
            }
            
            orderEntity.TotalAmound = totalAmount;
            
            // Add entities to repositories
            orderRepository.Add(orderEntity);
            
            foreach (var orderDetail in orderDetails)
            {
                orderDetailRepository.Add(orderDetail);
            }
            
            // Save changes
            var savedChanges = await _unitOfWork.SaveManualChangesAsync();
            
            return new OperationResult<bool>
            {
                IsError = savedChanges <= 0,
                Payload = savedChanges > 0,
                Message = savedChanges > 0 ? "Order created successfully" : "Failed to create order"
            };
        }
        catch (Exception ex)
        {
            return new OperationResult<bool>
            {
                IsError = true,
                Payload = false,
                Message = $"Error creating order: {ex.Message}"
            };
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
                return new OperationResult<bool>
                {
                    IsError = true,
                    Payload = false,
                    Message = "Order not found"
                };
            }
            
            // Update order properties using ReflectionHelper
            ReflectionHepler.UpdateProperties(request, existingOrder);
            
            // Update the entity
            repository.Update(existingOrder);
            
            // Save changes
            var savedChanges = await _unitOfWork.SaveManualChangesAsync();
            
            return new OperationResult<bool>
            {
                IsError = savedChanges <= 0,
                Payload = savedChanges > 0,
                Message = savedChanges > 0 ? "Order updated successfully" : "Failed to update order"
            };
        }
        catch (Exception ex)
        {
            return new OperationResult<bool>
            {
                IsError = true,
                Payload = false,
                Message = $"Error updating order: {ex.Message}"
            };
        }
    }
}
