using CPPR.Domain.Entities;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CPPR.BlazorWasm.Services
{
    public interface IDataService
    {
        event Action? DataLoaded;
        List<Category> Categories { get; set; }
        List<Dish> Dishes { get; set; }
        bool Success { get; set; }
        string ErrorMessage { get; set; }
        int TotalPages { get; set; }
        int CurrentPage { get; set; }
        Category? SelectedCategory { get; set; }
        
        Task GetProductListAsync(int pageNo = 1);
        Task GetCategoryListAsync();
    }
}
