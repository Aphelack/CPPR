using CPPR.Domain.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Project.Controllers;

public class CartController : Controller
{
    private readonly IProductService _productService;
    private readonly Cart _cart;

    public CartController(IProductService productService, Cart cart)
    {
        _productService = productService;
        _cart = cart;
    }

    public IActionResult Index()
    {
        return View(_cart);
    }

    [Authorize]
    [Route("[controller]/add/{id:int}")]
    public async Task<ActionResult> Add(int id, string returnUrl)
    {
        var data = await _productService.GetProductByIdAsync(id);
        if (data.Successfull)
        {
            _cart.AddToCart(data.Data!);
        }
        return Redirect(returnUrl);
    }

    [Authorize]
    [Route("[controller]/remove/{id:int}")]
    public IActionResult Remove(int id, string returnUrl)
    {
        _cart.RemoveItems(id);
        return Redirect(returnUrl);
    }

    [Authorize]
    public IActionResult Clear()
    {
        _cart.ClearAll();
        return RedirectToAction("Index");
    }
}
