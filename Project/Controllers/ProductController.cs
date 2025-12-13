using Microsoft.AspNetCore.Mvc;
using Project.Extensions;

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

    [Route("Catalog/{category?}")]
    public async Task<IActionResult> Index(string? category, int pageNo = 1)
    {
        var categoriesResponse = await _categoryService.GetCategoryListAsync();
        
        if (!categoriesResponse.Successfull)
            return NotFound(categoriesResponse.ErrorMessage);
        
        ViewData["Categories"] = categoriesResponse.Data;
        
        var currentCategoryName = category == null
            ? "Все"
            : categoriesResponse.Data?.FirstOrDefault(c => c.NormalizedName == category)?.Name;
        ViewData["CurrentCategory"] = category;
        
        var productResponse = await _productService.GetProductListAsync(category, pageNo);
        
        if (!productResponse.Successfull)
            return NotFound(productResponse.ErrorMessage);

        if (Request.IsAjaxRequest())
            return PartialView("_ListPartial", productResponse.Data);

        return View(productResponse.Data);
    }
}
