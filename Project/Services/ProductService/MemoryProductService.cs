namespace Project.Services.ProductService;

public class MemoryProductService : IProductService
{
    private List<Dish> _dishes;
    private List<Category> _categories;
    private readonly IConfiguration _config;

    public MemoryProductService(ICategoryService categoryService, IConfiguration config)
    {
        _config = config;
        _categories = categoryService.GetCategoryListAsync().Result.Data!;
        SetupData();
    }

    private void SetupData()
    {
        _dishes = new List<Dish>
        {
            new Dish { Id = 1, Name="Суп-харчо", Description="Очень острый, невкусный", Calories=200, Image="Images/soup.jpg", Category=_categories.Find(c=>c.NormalizedName=="soups") },
            new Dish { Id = 2, Name="Борщ", Description="Много сала, без сметаны", Calories=330, Image="Images/borscht.jpg", Category=_categories.Find(c=>c.NormalizedName=="soups") },
            new Dish { Id = 3, Name="Цезарь", Description="С курицей", Calories=400, Image="Images/caesar.jpg", Category=_categories.Find(c=>c.NormalizedName=="salads") },
            new Dish { Id = 4, Name="Оливье", Description="Новогодний", Calories=450, Image="Images/olivier.jpg", Category=_categories.Find(c=>c.NormalizedName=="salads") },
            new Dish { Id = 5, Name="Стейк", Description="Рибай", Calories=800, Image="Images/steak.jpg", Category=_categories.Find(c=>c.NormalizedName=="main-dishes") },
            new Dish { Id = 6, Name="Кола", Description="Zero", Calories=0, Image="Images/cola.jpg", Category=_categories.Find(c=>c.NormalizedName=="drinks") },
            new Dish { Id = 7, Name="Тирамису", Description="Вкусный", Calories=500, Image="Images/tiramisu.jpg", Category=_categories.Find(c=>c.NormalizedName=="desserts") }
        };
    }

    public Task<ResponseData<ListModel<Dish>>> GetProductListAsync(string? categoryNormalizedName, int pageNo = 1)
    {
        var itemsPerPage = _config.GetValue<int>("ItemsPerPage");
        if (itemsPerPage == 0) itemsPerPage = 3;

        var data = _dishes
            .Where(d => categoryNormalizedName == null || d.Category?.NormalizedName == categoryNormalizedName)
            .ToList();

        var totalItems = data.Count;
        var totalPages = (int)Math.Ceiling((double)totalItems / itemsPerPage);

        var listModel = new ListModel<Dish>
        {
            Items = data.Skip((pageNo - 1) * itemsPerPage).Take(itemsPerPage).ToList(),
            CurrentPage = pageNo,
            TotalPages = totalPages
        };

        return Task.FromResult(ResponseData<ListModel<Dish>>.Success(listModel));
    }

    public Task<ResponseData<Dish>> GetProductByIdAsync(int id)
    {
        var dish = _dishes.FirstOrDefault(d => d.Id == id);
        if (dish == null)
        {
            return Task.FromResult(ResponseData<Dish>.Error("Dish not found"));
        }
        return Task.FromResult(ResponseData<Dish>.Success(dish));
    }

    public Task UpdateProductAsync(int id, Dish product, IFormFile? formFile)
    {
        throw new NotImplementedException();
    }

    public Task DeleteProductAsync(int id)
    {
        throw new NotImplementedException();
    }

    public Task<ResponseData<Dish>> CreateProductAsync(Dish product, IFormFile? formFile)
    {
        throw new NotImplementedException();
    }
}
