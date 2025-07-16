using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using OrchidsShop.PresentationLayer.Models.Commons;

namespace OrchidsShop.PresentationLayer.Services;

public class ApiHelper
{
    private readonly HttpClient _httpClient;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiHelper(HttpClient httpClient, IHttpContextAccessor httpContextAccessor)
    {
        _httpClient = httpClient;
        _httpContextAccessor = httpContextAccessor;
        _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = null // Don't convert to camelCase for API responses
        };
    }

    private void AddAuthorizationHeader()
    {
        var jwtToken = _httpContextAccessor.HttpContext?.Session.GetString("JwtToken");
        if (!string.IsNullOrEmpty(jwtToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", jwtToken);
        }
    }

        // GET request - Returns ApiResponse<List<T>> for paginated results (Categories format)
        public async Task<ApiResponse<List<TData>>?> GetAsync<TData>(string url)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<List<TData>>
                    {
                        Success = false,
                        Message = errorMessage,
                        Errors = new List<string> { errorMessage }
                    };
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var apiResponse = JsonSerializer.Deserialize<ApiResponse<List<TData>>>(responseData, _jsonOptions);
                
                // Categories API doesn't include a Success field, so we set it based on HTTP success
                if (apiResponse != null)
                {
                    apiResponse.Success = true;
                }
                
                return apiResponse;
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<TData>>
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // GET request for BLL Operation Result format (Orchids format)
        public async Task<ApiResponse<List<TData>>?> GetBllAsync<TData>(string url)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return new ApiResponse<List<TData>>
                    {
                        Success = false,
                        Message = errorMessage,
                        Errors = new List<string> { errorMessage }
                    };
                }

                var responseData = await response.Content.ReadAsStringAsync();
                var bllResponse = JsonSerializer.Deserialize<BllOperationResponse<List<TData>>>(responseData, _jsonOptions);
                
                if (bllResponse == null)
                {
                    return new ApiResponse<List<TData>>
                    {
                        Success = false,
                        Message = "Failed to parse response",
                        Errors = new List<string> { "Failed to parse response" }
                    };
                }

                // Convert BLL response to standard ApiResponse format
                return new ApiResponse<List<TData>>
                {
                    Success = bllResponse.Success,
                    Message = bllResponse.Message,
                    Data = bllResponse.Payload,
                    Pagination = bllResponse.MetaData != null ? new PaginationModel
                    {
                        PageIndex = bllResponse.MetaData.PageIndex,
                        PageSize = bllResponse.MetaData.PageSize,
                        TotalItemsCount = bllResponse.MetaData.TotalItemsCount,
                        TotalPagesCount = bllResponse.MetaData.TotalPagesCount
                    } : null,
                    Errors = bllResponse.Errors?.Select(e => e.ToString()).ToList()
                };
            }
            catch (Exception ex)
            {
                return new ApiResponse<List<TData>>
                {
                    Success = false,
                    Message = ex.Message,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // GET request with query parameters (Categories format)
        public async Task<ApiResponse<List<TData>>?> GetWithQueryAsync<TData>(string baseUrl, object queryParams)
        {
            var url = BuildUrlWithQuery(baseUrl, queryParams);
            return await GetAsync<TData>(url);
        }

        // GET request with query parameters (BLL/Orchids format)
        public async Task<ApiResponse<List<TData>>?> GetBllWithQueryAsync<TData>(string baseUrl, object queryParams)
        {
            var url = BuildUrlWithQuery(baseUrl, queryParams);
            return await GetBllAsync<TData>(url);
        }

        // POST request - Returns ApiOperationResponse for create operations
        public async Task<ApiOperationResponse?> PostAsync<TRequest>(string url, TRequest data)
        {
            try
            {
                AddAuthorizationHeader();
                var content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return new ApiOperationResponse
                    {
                        Success = false,
                        Message = errorMessage,
                        Errors = new List<string> { errorMessage }
                    };
                }

                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ApiOperationResponse>(responseData, _jsonOptions);
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

        // POST request for login - Returns JWT token string on success
        public async Task<LoginResponse?> PostLoginAsync<TRequest>(string url, TRequest data)
        {
            try
            {
                // Don't add authorization header for login requests
                var content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                var responseData = await response.Content.ReadAsStringAsync();

                if (!response.IsSuccessStatusCode)
                {
                    // Try to parse error response in BLL format
                    try
                    {
                        var errorResponse = JsonSerializer.Deserialize<BllOperationResponse<LoginData>>(responseData, _jsonOptions);
                        return new LoginResponse
                        {
                            Success = false,
                            Message = errorResponse?.Message ?? responseData,
                            Data = null,
                            Errors = errorResponse?.Errors?.Select(e => e.ToString()).ToList() ?? new List<string> { responseData }
                        };
                    }
                    catch
                    {
                        return new LoginResponse
                        {
                            Success = false,
                            Message = responseData,
                            Data = null,
                            Errors = new List<string> { responseData }
                        };
                    }
                }

                // Parse successful BLL response format
                var bllResponse = JsonSerializer.Deserialize<BllOperationResponse<LoginData>>(responseData, _jsonOptions);
                
                if (bllResponse == null)
                {
                    return new LoginResponse
                    {
                        Success = false,
                        Message = "Failed to parse response",
                        Data = null,
                        Errors = new List<string> { "Failed to parse response" }
                    };
                }

                return new LoginResponse
                {
                    Success = !bllResponse.IsError,
                    Message = bllResponse.Message,
                    Data = bllResponse.Payload,
                    Errors = bllResponse.Errors?.Select(e => e.ToString()).ToList()
                };
            }
            catch (Exception ex)
            {
                return new LoginResponse
                {
                    Success = false,
                    Message = ex.Message,
                    Data = null,
                    Errors = new List<string> { ex.Message }
                };
            }
        }

        // PUT request - Returns ApiOperationResponse for update operations
        public async Task<ApiOperationResponse?> PutAsync<TRequest>(string url, TRequest data)
        {
            try
            {
                AddAuthorizationHeader();
                var content = new StringContent(JsonSerializer.Serialize(data, _jsonOptions), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return new ApiOperationResponse
                    {
                        Success = false,
                        Message = errorMessage,
                        Errors = new List<string> { errorMessage }
                    };
                }

                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ApiOperationResponse>(responseData, _jsonOptions);
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

        // DELETE request - Returns ApiOperationResponse for delete operations
        public async Task<ApiOperationResponse?> DeleteAsync(string url)
        {
            try
            {
                AddAuthorizationHeader();
                var response = await _httpClient.DeleteAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return new ApiOperationResponse
                    {
                        Success = false,
                        Message = errorMessage,
                        Errors = new List<string> { errorMessage }
                    };
                }

                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<ApiOperationResponse>(responseData, _jsonOptions);
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

        // Helper method to build URL with query parameters
        private string BuildUrlWithQuery(string baseUrl, object queryParams)
        {
            if (queryParams == null) return baseUrl;

            var properties = queryParams.GetType().GetProperties();
            var queryString = new List<string>();

            foreach (var prop in properties)
            {
                var value = prop.GetValue(queryParams);
                if (value != null)
                {
                    if (value is IEnumerable<Guid> guidList)
                    {
                        // Handle List<Guid> for Ids parameter - convert to comma-separated string
                        var guidStrings = guidList.Select(g => g.ToString());
                        queryString.Add($"{prop.Name}={Uri.EscapeDataString(string.Join(",", guidStrings))}");
                    }
                    else if (value is IEnumerable<string> stringList)
                    {
                        foreach (var item in stringList)
                        {
                            queryString.Add($"{prop.Name}={Uri.EscapeDataString(item)}");
                        }
                    }
                    else
                    {
                        queryString.Add($"{prop.Name}={Uri.EscapeDataString(value.ToString())}");
                    }
                }
            }

            return queryString.Count > 0 ? $"{baseUrl}?{string.Join("&", queryString)}" : baseUrl;
        }

        public async Task<byte[]> ConvertToByteArrayAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
}