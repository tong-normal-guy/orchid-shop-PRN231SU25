using OrchidsShop.PresentationLayer.Constants;
using OrchidsShop.PresentationLayer.Models.Categories;
using OrchidsShop.PresentationLayer.Models.Commons;

namespace OrchidsShop.PresentationLayer.Services;

/// <summary>
/// Service for handling category-related API operations
/// </summary>
public class CategoryApiService
{
    private readonly ApiHelper _apiHelper;
    private const string CategoryEndpoint = "orchid-categories";

    public CategoryApiService(ApiHelper apiHelper)
    {
        _apiHelper = apiHelper;
    }

    /// <summary>
    /// Lấy danh sách các danh mục với tùy chọn lọc và phân trang
    /// </summary>
    /// <param name="queryModel">Tham số truy vấn cho việc lọc và phân trang</param>
    /// <returns>Danh sách các danh mục</returns>
    public async Task<ApiResponse<List<CategoryModel>>?> GetCategoriesAsync(CategoryQueryModel? queryModel = null)
    {
        var baseUrl = StringValue.BaseUrl + CategoryEndpoint;
        
        if (queryModel != null)
        {
            return await _apiHelper.GetWithQueryAsync<CategoryModel>(baseUrl, queryModel);
        }
        
        return await _apiHelper.GetAsync<CategoryModel>(baseUrl);
    }

    /// <summary>
    /// Lấy danh mục theo ID
    /// </summary>
    /// <param name="id">ID của danh mục</param>
    /// <returns>Danh mục được tìm thấy</returns>
    public async Task<ApiResponse<List<CategoryModel>>?> GetCategoryByIdAsync(Guid id)
    {
        var queryModel = new CategoryQueryModel 
        { 
            Ids = new List<string> { id.ToString() } 
        };
        return await GetCategoriesAsync(queryModel);
    }

    /// <summary>
    /// Tìm kiếm danh mục theo tên
    /// </summary>
    /// <param name="searchTerm">Từ khóa tìm kiếm</param>
    /// <param name="pageNumber">Số trang</param>
    /// <param name="pageSize">Kích thước trang</param>
    /// <returns>Danh sách các danh mục phù hợp</returns>
    public async Task<ApiResponse<List<CategoryModel>>?> SearchCategoriesAsync(string searchTerm, int pageNumber = 1, int pageSize = 10)
    {
        var queryModel = new CategoryQueryModel 
        { 
            Search = searchTerm,
            PageNumber = pageNumber,
            PageSize = pageSize 
        };
        return await GetCategoriesAsync(queryModel);
    }

    /// <summary>
    /// Tạo nhiều danh mục mới
    /// </summary>
    /// <param name="categories">Danh sách các danh mục cần tạo</param>
    /// <returns>Kết quả của thao tác tạo</returns>
    public async Task<ApiOperationResponse?> CreateCategoriesAsync(List<CategoryRequestModel> categories)
    {
        var url = StringValue.BaseUrl + CategoryEndpoint;
        return await _apiHelper.PostAsync(url, categories);
    }

    /// <summary>
    /// Tạo một danh mục mới
    /// </summary>
    /// <param name="category">Danh mục cần tạo</param>
    /// <returns>Kết quả của thao tác tạo</returns>
    public async Task<ApiOperationResponse?> CreateCategoryAsync(CategoryRequestModel category)
    {
        return await CreateCategoriesAsync(new List<CategoryRequestModel> { category });
    }

    /// <summary>
    /// Cập nhật danh mục hoàn toàn
    /// </summary>
    /// <param name="id">ID của danh mục cần cập nhật</param>
    /// <param name="category">Dữ liệu danh mục mới</param>
    /// <returns>Kết quả của thao tác cập nhật</returns>
    public async Task<ApiOperationResponse?> UpdateCategoryAsync(Guid id, CategoryRequestModel category)
    {
        var url = $"{StringValue.BaseUrl}{CategoryEndpoint}/{id}";
        return await _apiHelper.PutAsync(url, category);
    }

    /// <summary>
    /// Cập nhật một phần danh mục
    /// </summary>
    /// <param name="id">ID của danh mục cần cập nhật</param>
    /// <param name="category">Dữ liệu danh mục cần cập nhật (chỉ các trường không null)</param>
    /// <returns>Kết quả của thao tác cập nhật</returns>
    public async Task<ApiOperationResponse?> PatchCategoryAsync(Guid id, CategoryRequestModel category)
    {
        var url = $"{StringValue.BaseUrl}{CategoryEndpoint}/{id}";
        
        // Create HttpRequestMessage for PATCH
        var httpClient = new HttpClient();
        var json = System.Text.Json.JsonSerializer.Serialize(category);
        var content = new StringContent(json, System.Text.Encoding.UTF8, "application/json");
        
        var request = new HttpRequestMessage(HttpMethod.Patch, url)
        {
            Content = content
        };

        try
        {
            var response = await httpClient.SendAsync(request);
            var responseContent = await response.Content.ReadAsStringAsync();
            
            if (!response.IsSuccessStatusCode)
            {
                return new ApiOperationResponse
                {
                    Success = false,
                    Message = responseContent,
                    Errors = new List<string> { responseContent }
                };
            }

            return System.Text.Json.JsonSerializer.Deserialize<ApiOperationResponse>(responseContent, new System.Text.Json.JsonSerializerOptions 
            { 
                PropertyNameCaseInsensitive = true 
            });
        }
        catch (Exception ex)
        {
            return new ApiOperationResponse
            {
                Success = false,
                Message = ex.Message,
                Errors = new List<string> { ex.Message }
            };
        }
    }

    /// <summary>
    /// Xóa danh mục
    /// </summary>
    /// <param name="id">ID của danh mục cần xóa</param>
    /// <returns>Kết quả của thao tác xóa</returns>
    public async Task<ApiOperationResponse?> DeleteCategoryAsync(Guid id)
    {
        var url = $"{StringValue.BaseUrl}{CategoryEndpoint}/{id}";
        return await _apiHelper.DeleteAsync(url);
    }

    /// <summary>
    /// Lấy tất cả danh mục cho dropdown/select
    /// </summary>
    /// <returns>Danh sách tất cả các danh mục được sắp xếp theo tên</returns>
    public async Task<ApiResponse<List<CategoryModel>>?> GetAllCategoriesForDropdownAsync()
    {
        var queryModel = new CategoryQueryModel 
        { 
            PageSize = 100, // Get a large number for dropdown
            SortColumn = "Name",
            SortDir = "Asc"
        };
        return await GetCategoriesAsync(queryModel);
    }
} 