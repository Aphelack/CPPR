namespace Project.Services.CategoryService;

public interface ICategoryService
{
    Task<ResponseData<List<Category>>> GetCategoryListAsync();
}
