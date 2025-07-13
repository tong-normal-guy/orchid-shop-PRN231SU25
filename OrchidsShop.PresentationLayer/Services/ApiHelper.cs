using System.Text;
using System.Text.Json;
using OrchidsShop.PresentationLayer.Models.Commons;

namespace OrchidsShop.PresentationLayer.Services;

public class ApiHelper
{
    private readonly HttpClient _httpClient;

        public ApiHelper(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // GET request
        public async Task<TResponse?> GetAsync<TResponse>(string url)
        {
            try
            {
                var response = await _httpClient.GetAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TResponse>(JsonSerializer.Serialize(new ErrorResponseModel()
                    {
                        Status = response.StatusCode.ToString(),
                        Message = errorMessage
                    }));
                }

                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseData,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Deserialize<TResponse>(JsonSerializer.Serialize(new ErrorResponseModel
                {
                    Status = "500", // Internal Server Error
                    Message = ex.Message
                }));
            }
        }

        // POST request
        public async Task<TResponse?> PostAsync<TRequest, TResponse>(string url, TRequest data)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
                var response = await _httpClient.PostAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TResponse>(JsonSerializer.Serialize(new ErrorResponseModel
                    {
                        Status = response.StatusCode.ToString(),
                        Message = errorMessage
                    }));
                }

                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseData,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Deserialize<TResponse>(JsonSerializer.Serialize(new ErrorResponseModel
                {
                    Status = "500", // Internal Server Error
                    Message = ex.Message
                }));
            }
        }

        // PUT request
        public async Task<TResponse?> PutAsync<TRequest, TResponse>(string url, TRequest data)
        {
            try
            {
                var content = new StringContent(JsonSerializer.Serialize(data), Encoding.UTF8, "application/json");
                var response = await _httpClient.PutAsync(url, content);

                if (!response.IsSuccessStatusCode)
                {
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    return JsonSerializer.Deserialize<TResponse>(JsonSerializer.Serialize(new ErrorResponseModel
                    {
                        Status = response.StatusCode.ToString(),
                        Message = errorMessage
                    }));
                }

                var responseData = await response.Content.ReadAsStringAsync();
                return JsonSerializer.Deserialize<TResponse>(responseData,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                return JsonSerializer.Deserialize<TResponse>(JsonSerializer.Serialize(new ErrorResponseModel
                {
                    Status = "500", // Internal Server Error
                    Message = ex.Message
                }));
            }
        }

        // DELETE request (returns bool)
        public async Task<bool> DeleteAsync(string url)
        {
            try
            {
                var response = await _httpClient.DeleteAsync(url);

                if (!response.IsSuccessStatusCode)
                {
                    // Log error message if needed
                    var errorMessage = await response.Content.ReadAsStringAsync();
                    Console.WriteLine($"DELETE failed: {errorMessage}");
                    return false; // Return false if deletion failed
                }

                return true; // Return true if deletion was successful
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Exception during DELETE: {ex.Message}");
                return false; // Return false if an exception occurred
            }
        }

        public async Task<byte[]> ConvertToByteArrayAsync(IFormFile file)
        {
            using var memoryStream = new MemoryStream();
            await file.CopyToAsync(memoryStream);
            return memoryStream.ToArray();
        }
}