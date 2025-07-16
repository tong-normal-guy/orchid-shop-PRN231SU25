namespace OrchidsShop.PresentationLayer.Models.Commons;

/// <summary>
/// Generic API response model for handling API responses from OrchidsShop.API
/// </summary>
/// <typeparam name="T">The type of data being returned</typeparam>
public class ApiResponse<T>
{
    public string? Message { get; set; }
    public T? Data { get; set; }
    public bool Success { get; set; }
    public PaginationModel? Pagination { get; set; }
    public List<string>? Errors { get; set; }
}

/// <summary>
/// API response model for operations that return boolean results
/// </summary>
public class ApiOperationResponse
{
    public string? Message { get; set; }
    public bool Success { get; set; }
    public List<string>? Errors { get; set; }
}

/// <summary>
/// Pagination metadata model
/// </summary>
public class PaginationModel
{
    public int PageNumber { get; set; }
    public int PageSize { get; set; }
    public int TotalRecords { get; set; }
    public int TotalPages { get; set; }
} 