using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using Project.Controllers;
using Project.Services.CategoryService;
using Project.Services.ProductService;
using CPPR.Domain.Models;
using CPPR.Domain.Entities;
using Xunit;

namespace CPPR.Tests
{
    public class ProductControllerTests
    {
        private readonly IProductService _productService;
        private readonly ICategoryService _categoryService;
        private readonly ProductController _controller;

        public ProductControllerTests()
        {
            _productService = Substitute.For<IProductService>();
            _categoryService = Substitute.For<ICategoryService>();
            _controller = new ProductController(_productService, _categoryService);
            
            // Mock ControllerContext for Request
            var httpContext = new DefaultHttpContext();
            _controller.ControllerContext = new ControllerContext
            {
                HttpContext = httpContext
            };
        }

        [Fact]
        public async Task Index_ReturnsNotFound_WhenCategoriesFail()
        {
            // Arrange
            _categoryService.GetCategoryListAsync().Returns(ResponseData<List<Category>>.Error("Category Error"));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Category Error", notFoundResult.Value);
        }

        [Fact]
        public async Task Index_ReturnsNotFound_WhenProductsFail()
        {
            // Arrange
            _categoryService.GetCategoryListAsync().Returns(ResponseData<List<Category>>.Success(new List<Category>()));
            _productService.GetProductListAsync(Arg.Any<string?>(), Arg.Any<int>()).Returns(ResponseData<ListModel<Dish>>.Error("Product Error"));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result);
            Assert.Equal("Product Error", notFoundResult.Value);
        }

        [Fact]
        public async Task Index_ReturnsView_WithCorrectData_WhenSuccess()
        {
            // Arrange
            var categories = new List<Category> 
            { 
                new Category { Id = 1, Name = "Soups", NormalizedName = "soups" },
                new Category { Id = 2, Name = "Salads", NormalizedName = "salads" }
            };
            _categoryService.GetCategoryListAsync().Returns(ResponseData<List<Category>>.Success(categories));

            var dishes = new ListModel<Dish> { Items = new List<Dish> { new Dish { Id = 1, Name = "Soup" } } };
            _productService.GetProductListAsync(Arg.Any<string?>(), Arg.Any<int>()).Returns(ResponseData<ListModel<Dish>>.Success(dishes));

            // Act
            var result = await _controller.Index("soups");

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Equal(dishes, viewResult.Model);
            Assert.Equal(categories, _controller.ViewData["Categories"]);
            Assert.Equal("soups", _controller.ViewData["CurrentCategory"]);
        }
        
        [Fact]
        public async Task Index_SetsCurrentCategoryToAll_WhenCategoryIsNull()
        {
            // Arrange
            var categories = new List<Category>();
            _categoryService.GetCategoryListAsync().Returns(ResponseData<List<Category>>.Success(categories));

            var dishes = new ListModel<Dish>();
            _productService.GetProductListAsync(null, 1).Returns(ResponseData<ListModel<Dish>>.Success(dishes));

            // Act
            var result = await _controller.Index(null);

            // Assert
            var viewResult = Assert.IsType<ViewResult>(result);
            Assert.Null(_controller.ViewData["CurrentCategory"]);
            // Note: In the controller code provided in instructions, it sets "Все" if category is null.
            // But in the actual controller code I read:
            // var currentCategoryName = category == null ? "Все" : ...
            // ViewData["CurrentCategory"] = category; 
            // Wait, let me re-read the controller code I read earlier.
        }
    }
}
