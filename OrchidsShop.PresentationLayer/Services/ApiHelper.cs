using System.Text;
using System.Text.Json;
using OrchidsShop.PresentationLayer.Models.Commons;

namespace OrchidsShop.PresentationLayer.Services;

public class ApiHelper
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions;

    public ApiHelper(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _jsonOptions = new JsonSerializerOptions 
        { 
            PropertyNameCaseInsensitive = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        };
    }

        // GET request - Returns ApiResponse<List<T>> for paginated results
        public async Task<ApiResponse<List<TData>>?> GetAsync<TData>(string url)
        {
            try
            {
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
                return JsonSerializer.Deserialize<ApiResponse<List<TData>>>(responseData, _jsonOptions);
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

        // GET request with query parameters
        public async Task<ApiResponse<List<TData>>?> GetWithQueryAsync<TData>(string baseUrl, object queryParams)
        {
            var url = BuildUrlWithQuery(baseUrl, queryParams);
            return await GetAsync<TData>(url);
        }

        // POST request - Returns ApiOperationResponse for create operations
        public async Task<ApiOperationResponse?> PostAsync<TRequest>(string url, TRequest data)
        {
            try
            {
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

        // PUT request - Returns ApiOperationResponse for update operations
        public async Task<ApiOperationResponse?> PutAsync<TRequest>(string url, TRequest data)
        {
            try
            {
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
                    if (value is IEnumerable<string> list)
                    {
                        foreach (var item in list)
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