using CPPR.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace Project.ViewComponents
{
    public class CartViewComponent : ViewComponent
    {
        private readonly Cart _cart;

        public CartViewComponent(Cart cart)
        {
            _cart = cart;
        }

        public IViewComponentResult Invoke()
        {
            return View(_cart);
        }
    }
}