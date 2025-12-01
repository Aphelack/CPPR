namespace Project.Services.ProductService;

public interface IProductService
{
    Task<ResponseData<ListModel<Dish>>> GetProductListAsync(string? categoryNormalizedName, int pageNo = 1);
    Task<ResponseData<Dish>> GetProductByIdAsync(int id);
    Task UpdateProductAsync(int id, Dish product, IFormFile? formFile);
    Task DeleteProductAsync(int id);
    Task<ResponseData<Dish>> CreateProductAsync(Dish product, IFormFile? formFile);
}
