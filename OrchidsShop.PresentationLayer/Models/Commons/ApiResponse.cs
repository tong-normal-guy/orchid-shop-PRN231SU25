using System.Text.Json.Serialization;

namespace OrchidsShop.PresentationLayer.Models.Commons;

/// <summary>
/// Generic API response model for handling API responses from OrchidsShop.API
/// This handles the standard response format used by categories
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
/// BLL Operation Result response model for handling orchids API responses
/// This handles the OperationResult format used by orchids
/// </summary>
/// <typeparam name="T">The type of payload being returned</typeparam>
public class BllOperationResponse<T>
{
    public string? StatusCode { get; set; }
    public string? Message { get; set; }
    public bool IsError { get; set; }
    public T? Payload { get; set; }
    public PaginationMetadata? MetaData { get; set; }
    public List<object>? Errors { get; set; }
    
    // Helper property to get Success status
    public bool Success => !IsError;
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
/// Pagination metadata model for standard API responses
/// </summary>
public class PaginationModel
{
    // JSON property mapping for API compatibility
    [JsonPropertyName("pageIndex")]
    public int PageIndex { get; set; }
    
    [JsonPropertyName("pageSize")] 
    public int PageSize { get; set; }
    
    [JsonPropertyName("totalItemsCount")]
    public int TotalItemsCount { get; set; }
    
    [JsonPropertyName("totalPagesCount")]
    public int TotalPagesCount { get; set; }
    
    // Computed properties for consistent interface
    public int PageNumber => PageIndex + 1;
    public int TotalRecords => TotalItemsCount; 
    public int TotalPages => TotalPagesCount;
}

/// <summary>
/// Response model for login operations that return JWT tokens
/// </summary>
public class LoginResponse
{
    public bool Success { get; set; }
    public string? Message { get; set; }
    public LoginData? Data { get; set; }
    public List<string>? Errors { get; set; }
}

/// <summary>
/// Login data containing token and user information
/// </summary>
public class LoginData
{
    public string? Token { get; set; }
    public string? Email { get; set; }
    public string? Name { get; set; }
    public string? Role { get; set; }
    public Guid? UserId { get; set; }
}

/// <summary>
/// Pagination metadata model for BLL Operation Result responses
/// </summary>
public class PaginationMetadata
{
    public int TotalItemsCount { get; set; }
    public int PageSize { get; set; }
    public int PageIndex { get; set; }
    public int TotalPagesCount { get; set; }
    
    // Helper properties to convert to standard pagination model
    public int PageNumber => PageIndex + 1;
    public int TotalRecords => TotalItemsCount;
    public int TotalPages => TotalPagesCount;
} 