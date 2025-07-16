using OrchidsShop.PresentationLayer.Constants;
using OrchidsShop.PresentationLayer.Models.Commons;
using OrchidsShop.PresentationLayer.Models.Orders;

namespace OrchidsShop.PresentationLayer.Services;

/// <summary>
/// Service for handling order-related API operations
/// </summary>
public class OrderApiService
{
    private readonly ApiHelper _apiHelper;
    private const string OrderEndpoint = "orders";

    public OrderApiService(ApiHelper apiHelper)
    {
        _apiHelper = apiHelper;
    }

    /// <summary>
    /// Lấy danh sách đơn hàng với tùy chọn lọc và phân trang
    /// </summary>
    /// <param name="queryModel">Tham số truy vấn cho việc lọc và phân trang</param>
    /// <returns>Danh sách các đơn hàng</returns>
    public async Task<ApiResponse<List<OrderModel>>?> GetOrdersAsync(OrderQueryModel? queryModel = null)
    {
        var baseUrl = StringValue.BaseUrl + OrderEndpoint;
        
        if (queryModel != null)
        {
            return await _apiHelper.GetWithQueryAsync<OrderModel>(baseUrl, queryModel);
        }
        
        return await _apiHelper.GetAsync<OrderModel>(baseUrl);
    }

    /// <summary>
    /// Lấy đơn hàng theo ID
    /// </summary>
    /// <param name="id">ID của đơn hàng</param>
    /// <returns>Đơn hàng được tìm thấy</returns>
    public async Task<ApiResponse<List<OrderModel>>?> GetOrderByIdAsync(Guid id)
    {
        var url = $"{StringValue.BaseUrl}{OrderEndpoint}/{id}";
        return await _apiHelper.GetAsync<OrderModel>(url);
    }

    /// <summary>
    /// Lấy đơn hàng của khách hàng cụ thể
    /// </summary>
    /// <param name="accountId">ID của khách hàng</param>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <returns>Danh sách đơn hàng của khách hàng</returns>
    public async Task<ApiResponse<List<OrderModel>>?> GetOrdersByCustomerAsync(Guid accountId, int pageNumber = 1, int pageSize = 10)
    {
        var queryModel = new OrderQueryModel 
        { 
            AccountId = accountId,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortColumn = "OrderDate",
            SortDir = "Desc"
        };
        return await GetOrdersAsync(queryModel);
    }

    /// <summary>
    /// Lọc đơn hàng theo trạng thái
    /// </summary>
    /// <param name="status">Trạng thái đơn hàng</param>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <returns>Danh sách đơn hàng theo trạng thái</returns>
    public async Task<ApiResponse<List<OrderModel>>?> GetOrdersByStatusAsync(string status, int pageNumber = 1, int pageSize = 10)
    {
        var queryModel = new OrderQueryModel 
        { 
            Status = status,
            PageNumber = pageNumber,
            PageSize = pageSize 
        };
        return await GetOrdersAsync(queryModel);
    }

    /// <summary>
    /// Lọc đơn hàng theo khoảng thời gian
    /// </summary>
    /// <param name="startDate">Ngày bắt đầu</param>
    /// <param name="endDate">Ngày kết thúc</param>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <returns>Danh sách đơn hàng trong khoảng thời gian</returns>
    public async Task<ApiResponse<List<OrderModel>>?> GetOrdersByDateRangeAsync(DateTime startDate, DateTime endDate, int pageNumber = 1, int pageSize = 10)
    {
        var queryModel = new OrderQueryModel 
        { 
            StartDate = startDate,
            EndDate = endDate,
            PageNumber = pageNumber,
            PageSize = pageSize 
        };
        return await GetOrdersAsync(queryModel);
    }

    /// <summary>
    /// Tạo đơn hàng mới
    /// </summary>
    /// <param name="order">Dữ liệu đơn hàng cần tạo</param>
    /// <returns>Kết quả của thao tác tạo</returns>
    public async Task<ApiOperationResponse?> CreateOrderAsync(OrderRequestModel order)
    {
        var url = StringValue.BaseUrl + OrderEndpoint;
        return await _apiHelper.PostAsync(url, order);
    }

    /// <summary>
    /// Cập nhật đơn hàng
    /// </summary>
    /// <param name="order">Dữ liệu đơn hàng cần cập nhật (phải có ID)</param>
    /// <returns>Kết quả của thao tác cập nhật</returns>
    public async Task<ApiOperationResponse?> UpdateOrderAsync(OrderRequestModel order)
    {
        var url = StringValue.BaseUrl + OrderEndpoint;
        return await _apiHelper.PutAsync(url, order);
    }

    /// <summary>
    /// Cập nhật trạng thái đơn hàng
    /// </summary>
    /// <param name="orderId">ID của đơn hàng</param>
    /// <param name="status">Trạng thái mới</param>
    /// <returns>Kết quả của thao tác cập nhật</returns>
    public async Task<ApiOperationResponse?> UpdateOrderStatusAsync(Guid orderId, string status)
    {
        var updateRequest = new OrderRequestModel
        {
            Id = orderId,
            Status = status
        };
        return await UpdateOrderAsync(updateRequest);
    }

    /// <summary>
    /// Thêm sản phẩm vào đơn hàng
    /// </summary>
    /// <param name="orderId">ID của đơn hàng</param>
    /// <param name="orchidId">ID của hoa lan</param>
    /// <param name="quantity">Số lượng</param>
    /// <returns>Kết quả của thao tác thêm</returns>
    public async Task<ApiOperationResponse?> AddItemToOrderAsync(Guid orderId, Guid orchidId, int quantity)
    {
        var orderDetail = new OrderDetailRequestModel
        {
            OrchidId = orchidId,
            Quantity = quantity
        };

        var updateRequest = new OrderRequestModel
        {
            Id = orderId,
            OrderDetails = new List<OrderDetailRequestModel> { orderDetail }
        };

        return await UpdateOrderAsync(updateRequest);
    }

    /// <summary>
    /// Cập nhật số lượng sản phẩm trong đơn hàng
    /// </summary>
    /// <param name="orderId">ID của đơn hàng</param>
    /// <param name="orderDetailId">ID của chi tiết đơn hàng</param>
    /// <param name="newQuantity">Số lượng mới</param>
    /// <returns>Kết quả của thao tác cập nhật</returns>
    public async Task<ApiOperationResponse?> UpdateOrderItemQuantityAsync(Guid orderId, Guid orderDetailId, int newQuantity)
    {
        var orderDetail = new OrderDetailRequestModel
        {
            Id = orderDetailId,
            Quantity = newQuantity
        };

        var updateRequest = new OrderRequestModel
        {
            Id = orderId,
            OrderDetails = new List<OrderDetailRequestModel> { orderDetail }
        };

        return await UpdateOrderAsync(updateRequest);
    }

    /// <summary>
    /// Xóa đơn hàng
    /// </summary>
    /// <param name="orderId">ID của đơn hàng cần xóa</param>
    /// <returns>Kết quả của thao tác xóa</returns>
    public async Task<ApiOperationResponse?> DeleteOrderAsync(Guid orderId)
    {
        var deleteRequest = new OrderRequestModel
        {
            Id = orderId
        };
        
        var url = StringValue.BaseUrl + OrderEndpoint;
        return await _apiHelper.DeleteAsync(url);
    }

    /// <summary>
    /// Tạo đơn hàng đơn giản với một sản phẩm
    /// </summary>
    /// <param name="accountId">ID của khách hàng</param>
    /// <param name="orchidId">ID của hoa lan</param>
    /// <param name="quantity">Số lượng</param>
    /// <returns>Kết quả của thao tác tạo</returns>
    public async Task<ApiOperationResponse?> CreateSimpleOrderAsync(Guid accountId, Guid orchidId, int quantity)
    {
        var orderDetail = new OrderDetailRequestModel
        {
            OrchidId = orchidId,
            Quantity = quantity
        };

        var order = new OrderRequestModel
        {
            AccountId = accountId,
            OrderDetails = new List<OrderDetailRequestModel> { orderDetail }
        };

        return await CreateOrderAsync(order);
    }

    /// <summary>
    /// Tạo đơn hàng từ giỏ hàng
    /// </summary>
    /// <param name="accountId">ID của khách hàng</param>
    /// <param name="cartItems">Danh sách sản phẩm trong giỏ hàng</param>
    /// <returns>Kết quả của thao tác tạo</returns>
    public async Task<ApiOperationResponse?> CreateOrderFromCartAsync(Guid accountId, List<OrderDetailRequestModel> cartItems)
    {
        var order = new OrderRequestModel
        {
            AccountId = accountId,
            OrderDetails = cartItems
        };

        return await CreateOrderAsync(order);
    }

    /// <summary>
    /// Lấy lịch sử đơn hàng của khách hàng
    /// </summary>
    /// <param name="accountId">ID của khách hàng</param>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <returns>Danh sách đơn hàng theo thời gian giảm dần</returns>
    public async Task<ApiResponse<List<OrderModel>>?> GetCustomerOrderHistoryAsync(Guid accountId, int pageNumber = 1, int pageSize = 10)
    {
        return await GetOrdersByCustomerAsync(accountId, pageNumber, pageSize);
    }
} 