using Microsoft.AspNetCore.Mvc;

namespace Project.Controllers;

public class ProductController : Controller
{
    private readonly IProductService _productService;
    private readonly ICategoryService _categoryService;

    public ProductController(IProductService productService, ICategoryService categoryService)
    {
        _productService = productService;
        _categoryService = categoryService;
    }

    public async Task<IActionResult> Index(string? category, int pageNo = 1)
    {
        var productResponse = await _productService.GetProductListAsync(category, pageNo);
        if (!productResponse.Successfull)
            return NotFound(productResponse.ErrorMessage);

        var categoriesResponse = await _categoryService.GetCategoryListAsync();
        ViewData["Categories"] = categoriesResponse.Data;
        ViewData["CurrentCategory"] = category;

        return View(productResponse.Data);
    }
}
