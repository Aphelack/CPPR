using CPPR.Domain.Entities;
using CPPR.Domain.Models;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Project.Services.ProductService;

namespace Project.Areas.Admin.Pages.Product
{
    public class IndexModel : PageModel
    {
        private readonly IProductService _productService;

        public IndexModel(IProductService productService)
        {
            _productService = productService;
        }

        public List<Dish> Dish { get; set; } = new();
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;

        public async Task OnGetAsync(int pageNo = 1)
        {
            var response = await _productService.GetProductListAsync(null, pageNo);
            if (response.Successfull)
            {
                Dish = response.Data!.Items;
                CurrentPage = response.Data.CurrentPage;
                TotalPages = response.Data.TotalPages;
            }
        }
    }
}
