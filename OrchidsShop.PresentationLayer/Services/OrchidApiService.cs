using OrchidsShop.PresentationLayer.Constants;
using OrchidsShop.PresentationLayer.Models.Commons;
using OrchidsShop.PresentationLayer.Models.Orchids;

namespace OrchidsShop.PresentationLayer.Services;

/// <summary>
/// Service for handling orchid-related API operations
/// </summary>
public class OrchidApiService
{
    private readonly ApiHelper _apiHelper;
    private const string OrchidEndpoint = "orchids";

    public OrchidApiService(ApiHelper apiHelper)
    {
        _apiHelper = apiHelper;
    }

    /// <summary>
    /// Lấy danh sách các hoa lan với tùy chọn lọc và phân trang
    /// </summary>
    /// <param name="queryModel">Tham số truy vấn cho việc lọc và phân trang</param>
    /// <returns>Danh sách các hoa lan</returns>
    public async Task<ApiResponse<List<OrchidModel>>?> GetOrchidsAsync(OrchidQueryModel? queryModel = null)
    {
        var baseUrl = StringValue.BaseUrl + OrchidEndpoint;
        
        if (queryModel != null)
        {
            return await _apiHelper.GetWithQueryAsync<OrchidModel>(baseUrl, queryModel);
        }
        
        return await _apiHelper.GetAsync<OrchidModel>(baseUrl);
    }

    /// <summary>
    /// Lấy hoa lan theo ID
    /// </summary>
    /// <param name="id">ID của hoa lan</param>
    /// <returns>Hoa lan được tìm thấy</returns>
    public async Task<ApiResponse<List<OrchidModel>>?> GetOrchidByIdAsync(Guid id)
    {
        var queryModel = new OrchidQueryModel 
        { 
            Ids = id.ToString() 
        };
        return await GetOrchidsAsync(queryModel);
    }

    /// <summary>
    /// Tìm kiếm hoa lan theo tên hoặc mô tả
    /// </summary>
    /// <param name="searchTerm">Từ khóa tìm kiếm</param>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <returns>Danh sách các hoa lan phù hợp</returns>
    public async Task<ApiResponse<List<OrchidModel>>?> SearchOrchidsAsync(string searchTerm, int pageNumber = 1, int pageSize = 12)
    {
        var queryModel = new OrchidQueryModel 
        { 
            Search = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize 
        };
        return await GetOrchidsAsync(queryModel);
    }

    /// <summary>
    /// Lấy hoa lan theo danh mục
    /// </summary>
    /// <param name="categoryNames">Tên các danh mục (phân cách bằng dấu phẩy)</param>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <returns>Danh sách các hoa lan thuộc danh mục</returns>
    public async Task<ApiResponse<List<OrchidModel>>?> GetOrchidsByCategoryAsync(string categoryNames, int pageNumber = 1, int pageSize = 12)
    {
        var queryModel = new OrchidQueryModel 
        { 
            Categories = categoryNames,
            PageNumber = pageNumber,
            PageSize = pageSize 
        };
        return await GetOrchidsAsync(queryModel);
    }

    /// <summary>
    /// Lọc hoa lan theo khoảng giá
    /// </summary>
    /// <param name="minPrice">Giá tối thiểu</param>
    /// <param name="maxPrice">Giá tối đa</param>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <returns>Danh sách các hoa lan trong khoảng giá</returns>
    public async Task<ApiResponse<List<OrchidModel>>?> GetOrchidsByPriceRangeAsync(decimal? minPrice, decimal? maxPrice, int pageNumber = 1, int pageSize = 12)
    {
        var queryModel = new OrchidQueryModel 
        { 
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            PageNumber = pageNumber,
            PageSize = pageSize 
        };
        return await GetOrchidsAsync(queryModel);
    }

    /// <summary>
    /// Lọc hoa lan theo loại (tự nhiên/nhân tạo)
    /// </summary>
    /// <param name="isNatural">True cho hoa lan tự nhiên, false cho nhân tạo</param>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <returns>Danh sách các hoa lan theo loại</returns>
    public async Task<ApiResponse<List<OrchidModel>>?> GetOrchidsByTypeAsync(bool isNatural, int pageNumber = 1, int pageSize = 12)
    {
        var queryModel = new OrchidQueryModel 
        { 
            IsNarutal = isNatural,
            PageNumber = pageNumber,
            PageSize = pageSize 
        };
        return await GetOrchidsAsync(queryModel);
    }

    /// <summary>
    /// Tạo hoa lan mới
    /// </summary>
    /// <param name="orchid">Dữ liệu hoa lan cần tạo</param>
    /// <returns>Kết quả của thao tác tạo</returns>
    public async Task<ApiOperationResponse?> CreateOrchidAsync(OrchidRequestModel orchid)
    {
        var url = StringValue.BaseUrl + OrchidEndpoint;
        return await _apiHelper.PostAsync(url, orchid);
    }

    /// <summary>
    /// Cập nhật hoa lan
    /// </summary>
    /// <param name="orchid">Dữ liệu hoa lan cần cập nhật (phải có ID)</param>
    /// <returns>Kết quả của thao tác cập nhật</returns>
    public async Task<ApiOperationResponse?> UpdateOrchidAsync(OrchidRequestModel orchid)
    {
        var url = StringValue.BaseUrl + OrchidEndpoint;
        return await _apiHelper.PutAsync(url, orchid);
    }

    /// <summary>
    /// Xóa hoa lan
    /// </summary>
    /// <param name="id">ID của hoa lan cần xóa</param>
    /// <returns>Kết quả của thao tác xóa</returns>
    public async Task<ApiOperationResponse?> DeleteOrchidAsync(Guid id)
    {
        var url = $"{StringValue.BaseUrl}{OrchidEndpoint}/{id}";
        return await _apiHelper.DeleteAsync(url);
    }

    /// <summary>
    /// Lấy danh sách hoa lan cho trang sản phẩm chính
    /// </summary>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <param name="sortBy">Cột sắp xếp (Name, Price, etc.)</param>
    /// <param name="sortDirection">Hướng sắp xếp (Asc/Desc)</param>
    /// <returns>Danh sách hoa lan được sắp xếp</returns>
    public async Task<ApiResponse<List<OrchidModel>>?> GetOrchidsForCatalogAsync(int pageNumber = 1, int pageSize = 12, string sortBy = "Name", string sortDirection = "Asc")
    {
        var queryModel = new OrchidQueryModel 
        { 
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortColumn = sortBy,
            SortDir = sortDirection
        };
        return await GetOrchidsAsync(queryModel);
    }

    /// <summary>
    /// Tìm kiếm hoa lan nâng cao với nhiều bộ lọc
    /// </summary>
    /// <param name="search">Từ khóa tìm kiếm</param>
    /// <param name="categories">Danh mục (phân cách bằng dấu phẩy)</param>
    /// <param name="isNatural">Loại hoa lan</param>
    /// <param name="minPrice">Giá tối thiểu</param>
    /// <param name="maxPrice">Giá tối đa</param>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <param name="sortBy">Cột sắp xếp</param>
    /// <param name="sortDirection">Hướng sắp xếp</param>
    /// <returns>Danh sách hoa lan phù hợp với các bộ lọc</returns>
    public async Task<ApiResponse<List<OrchidModel>>?> AdvancedSearchOrchidsAsync(
        string? search = null,
        string? categories = null,
        bool? isNatural = null,
        decimal? minPrice = null,
        decimal? maxPrice = null,
        int pageNumber = 1,
        int pageSize = 12,
        string sortBy = "Name",
        string sortDirection = "Asc")
    {
        var queryModel = new OrchidQueryModel 
        { 
            Search = search,
            Categories = categories,
            IsNarutal = isNatural,
            MinPrice = minPrice,
            MaxPrice = maxPrice,
            PageNumber = pageNumber,
            PageSize = pageSize,
            SortColumn = sortBy,
            SortDir = sortDirection
        };
        return await GetOrchidsAsync(queryModel);
    }
} 