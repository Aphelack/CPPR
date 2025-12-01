using System.Text.Json;
using CPPR.Domain.Entities;
using CPPR.Domain.Models;
using Project.Services.Authorization;

namespace Project.Services.ProductService
{
    public class ApiProductService : IProductService
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger<ApiProductService> _logger;
        private readonly JsonSerializerOptions _serializerOptions;
        private readonly ITokenAccessor _tokenAccessor;

        public ApiProductService(
            HttpClient httpClient, 
            ILogger<ApiProductService> logger,
            ITokenAccessor tokenAccessor)
        {
            _httpClient = httpClient;
            _logger = logger;
            _tokenAccessor = tokenAccessor;
            _serializerOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<ResponseData<Dish>> CreateProductAsync(Dish product, IFormFile? formFile)
        {
            await _tokenAccessor.SetAuthorizationHeaderAsync(_httpClient);
            
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Post,
                RequestUri = new Uri(_httpClient.BaseAddress!, "dish")
            };

            var content = new MultipartFormDataContent();

            if (formFile != null)
            {
                var streamContent = new StreamContent(formFile.OpenReadStream());
                content.Add(streamContent, "file", formFile.FileName);
            }

            var data = new StringContent(JsonSerializer.Serialize(product));
            content.Add(data, "dish");

            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ResponseData<Dish>>(_serializerOptions)
                       ?? ResponseData<Dish>.Error("Error reading response");
            }

            _logger.LogError($"-----> object not created. Error: {response.StatusCode}");
            return ResponseData<Dish>.Error($"Object not created. Error: {response.StatusCode}");
        }

        public async Task DeleteProductAsync(int id)
        {
            await _tokenAccessor.SetAuthorizationHeaderAsync(_httpClient);
            
            var response = await _httpClient.DeleteAsync($"dish/{id}");
            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"-----> object not deleted. Error: {response.StatusCode}");
            }
        }

        public async Task<ResponseData<Dish>> GetProductByIdAsync(int id)
        {
            var response = await _httpClient.GetAsync($"dish/{id}");
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ResponseData<Dish>>(_serializerOptions)
                       ?? ResponseData<Dish>.Error("Error reading response");
            }
            return ResponseData<Dish>.Error($"Object not found. Error: {response.StatusCode}");
        }

        public async Task<ResponseData<ListModel<Dish>>> GetProductListAsync(string? categoryNormalizedName, int pageNo = 1)
        {
            var urlString = new System.Text.StringBuilder("dish");
            if (categoryNormalizedName != null)
            {
                urlString.Append($"/{categoryNormalizedName}");
            }
            if (pageNo > 1)
            {
                urlString.Append($"?pageNo={pageNo}");
            }

            var response = await _httpClient.GetAsync(urlString.ToString());
            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ResponseData<ListModel<Dish>>>(_serializerOptions)
                       ?? ResponseData<ListModel<Dish>>.Error("Error reading response");
            }
            return ResponseData<ListModel<Dish>>.Error($"Data not loaded. Error: {response.StatusCode}");
        }

        public async Task UpdateProductAsync(int id, Dish product, IFormFile? formFile)
        {
            await _tokenAccessor.SetAuthorizationHeaderAsync(_httpClient);
            
            var request = new HttpRequestMessage
            {
                Method = HttpMethod.Put,
                RequestUri = new Uri(_httpClient.BaseAddress!, $"dish/{id}")
            };

            var content = new MultipartFormDataContent();

            if (formFile != null)
            {
                var streamContent = new StreamContent(formFile.OpenReadStream());
                content.Add(streamContent, "file", formFile.FileName);
            }

            var data = new StringContent(JsonSerializer.Serialize(product));
            content.Add(data, "dish");

            request.Content = content;

            var response = await _httpClient.SendAsync(request);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError($"-----> object not updated. Error: {response.StatusCode}");
            }
        }
    }
}
