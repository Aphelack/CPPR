using CPPR.Domain.Entities;
using CPPR.Domain.Models;
using System.Net.Http.Json;
using System.Text.Json;

namespace CPPR.BlazorWasm.Services
{
    public class DataService : IDataService
    {
        private readonly HttpClient _httpClient;

        public DataService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public event Action? DataLoaded;
        public List<Category> Categories { get; set; } = new();
        public List<Dish> Dishes { get; set; } = new();
        public bool Success { get; set; } = true;
        public string ErrorMessage { get; set; } = string.Empty;
        public int TotalPages { get; set; } = 1;
        public int CurrentPage { get; set; } = 1;
        public Category? SelectedCategory { get; set; }

        public async Task GetCategoryListAsync()
        {
            try
            {
                var result = await _httpClient.GetFromJsonAsync<ResponseData<List<Category>>>("api/Categories");
                if (result != null && result.Successfull && result.Data != null)
                {
                    Categories = result.Data;
                    DataLoaded?.Invoke();
                }
                else
                {
                    Success = false;
                    ErrorMessage = result?.ErrorMessage ?? "Error loading categories";
                }
            }
            catch (Exception ex)
            {
                Success = false;
                ErrorMessage = ex.Message;
            }
        }

        public async Task GetProductListAsync(int pageNo = 1)
        {
            var url = "api/dish";
            if (SelectedCategory != null)
            {
                url += $"/{SelectedCategory.NormalizedName}";
            }
            url += $"?pageNo={pageNo}";

            try
            {
                var result = await _httpClient.GetFromJsonAsync<ResponseData<ListModel<Dish>>>(url);
                if (result != null && result.Successfull && result.Data != null)
                {
                    Dishes = result.Data.Items;
                    TotalPages = result.Data.TotalPages;
                    CurrentPage = result.Data.CurrentPage;
                    Success = true;
                    DataLoaded?.Invoke();
                }
                else
                {
                    Success = false;
                    ErrorMessage = result?.ErrorMessage ?? "Error loading dishes";
                    Dishes = new List<Dish>();
                }
            }
            catch (Exception ex)
            {
                Success = false;
                ErrorMessage = ex.Message;
                Dishes = new List<Dish>();
            }
        }
    }
}
