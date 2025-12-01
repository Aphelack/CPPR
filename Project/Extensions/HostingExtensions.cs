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
        builder.Services.AddScoped<IProductService, MemoryProductService>();
    }
}
