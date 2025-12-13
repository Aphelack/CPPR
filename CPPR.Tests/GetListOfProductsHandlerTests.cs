using CPPR.API.Data;
using CPPR.API.Use_Cases;
using CPPR.Domain.Entities;
using CPPR.Domain.Models;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using System.Data.Common;
using Xunit;

namespace CPPR.Tests
{
    public class GetListOfProductsHandlerTests : IDisposable
    {
        private readonly DbConnection _connection;
        private readonly DbContextOptions<AppDbContext> _contextOptions;

        public GetListOfProductsHandlerTests()
        {
            _connection = new SqliteConnection("Filename=:memory:");
            _connection.Open();

            _contextOptions = new DbContextOptionsBuilder<AppDbContext>()
                .UseSqlite(_connection)
                .Options;

            using var context = new AppDbContext(_contextOptions);

            if (context.Database.EnsureCreated())
            {
                var cat1 = new Category { Id = 1, Name = "Cat1", NormalizedName = "cat1" };
                var cat2 = new Category { Id = 2, Name = "Cat2", NormalizedName = "cat2" };
                
                context.Categories.AddRange(cat1, cat2);

                context.Dishes.AddRange(
                    new Dish { Id = 1, Name = "Dish1", Category = cat1, Calories = 100 },
                    new Dish { Id = 2, Name = "Dish2", Category = cat1, Calories = 200 },
                    new Dish { Id = 3, Name = "Dish3", Category = cat1, Calories = 300 },
                    new Dish { Id = 4, Name = "Dish4", Category = cat2, Calories = 400 },
                    new Dish { Id = 5, Name = "Dish5", Category = cat2, Calories = 500 }
                );

                context.SaveChanges();
            }
        }

        AppDbContext CreateContext() => new AppDbContext(_contextOptions);

        public void Dispose() => _connection.Dispose();

        [Fact]
        public async Task Handle_ReturnsFirstPageOfThreeItems_WhenNoCategory()
        {
            using var context = CreateContext();
            var handler = new GetListOfProductsHandler(context);
            var request = new GetListOfProducts(null, 1, 3);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.True(result.Successfull);
            Assert.Equal(1, result.Data.CurrentPage);
            Assert.Equal(3, result.Data.Items.Count);
            Assert.Equal(2, result.Data.TotalPages);
            Assert.Equal("Dish1", result.Data.Items[0].Name);
        }

        [Fact]
        public async Task Handle_ReturnsCorrectPage_WhenPageIsSpecified()
        {
            using var context = CreateContext();
            var handler = new GetListOfProductsHandler(context);
            var request = new GetListOfProducts(null, 2, 3);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.True(result.Successfull);
            Assert.Equal(2, result.Data.CurrentPage);
            Assert.Equal(2, result.Data.Items.Count);
            Assert.Equal("Dish4", result.Data.Items[0].Name);
        }

        [Fact]
        public async Task Handle_FiltersByCategory()
        {
            using var context = CreateContext();
            var handler = new GetListOfProductsHandler(context);
            var request = new GetListOfProducts("cat1", 1, 3);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.True(result.Successfull);
            Assert.Equal(3, result.Data.Items.Count);
            Assert.All(result.Data.Items, d => Assert.Equal("Cat1", d.Category.Name));
        }

        [Fact]
        public async Task Handle_ReturnsError_WhenPageNumberTooHigh()
        {
            using var context = CreateContext();
            var handler = new GetListOfProductsHandler(context);
            var request = new GetListOfProducts(null, 10, 3);

            var result = await handler.Handle(request, CancellationToken.None);

            Assert.False(result.Successfull);
            Assert.Equal("No such page", result.ErrorMessage);
        }
    }
}
