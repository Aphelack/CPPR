namespace Project.Services.CategoryService;

public class MemoryCategoryService : ICategoryService
{
    public Task<ResponseData<List<Category>>> GetCategoryListAsync()
    {
        var categories = new List<Category>
        {
            new Category {Id=1, Name="Стартеры", NormalizedName="starters"},
            new Category {Id=2, Name="Салаты", NormalizedName="salads"},
            new Category {Id=3, Name="Супы", NormalizedName="soups"},
            new Category {Id=4, Name="Основные блюда", NormalizedName="main-dishes"},
            new Category {Id=5, Name="Напитки", NormalizedName="drinks"},
            new Category {Id=6, Name="Десерты", NormalizedName="desserts"}
        };

        return Task.FromResult(ResponseData<List<Category>>.Success(categories));
    }
}
