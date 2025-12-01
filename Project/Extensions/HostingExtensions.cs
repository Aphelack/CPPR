using CPPR.Domain.Entities;
using CPPR.Domain.Models;
using Project.Services.CategoryService;
using Project.Services.ProductService;

namespace Project.Extensions;

public static class HostingExtensions
{
    public static void RegisterCustomServices(this WebApplicationBuilder builder)
    {
        builder.Services.AddScoped<ICategoryService, MemoryCategoryService>();
        
        builder.Services.AddHttpClient<IProductService, ApiProductService>(opt =>
        {
            opt.BaseAddress = new Uri("http://localhost:5002/api/");
        });
    }
}
