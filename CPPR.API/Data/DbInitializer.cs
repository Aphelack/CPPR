using CPPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPPR.API.Data
{
    public static class DbInitializer
    {
        public static async Task SeedData(WebApplication app)
        {
            using var scope = app.Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            
            await context.Database.MigrateAsync();

            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new Category { Name = "Soups", NormalizedName = "soups" },
                    new Category { Name = "Main Dishes", NormalizedName = "main-dishes" },
                    new Category { Name = "Drinks", NormalizedName = "drinks" },
                    new Category { Name = "Desserts", NormalizedName = "desserts" }
                };
                await context.Categories.AddRangeAsync(categories);
                await context.SaveChangesAsync();
            }

            if (!context.Dishes.Any())
            {
                var soups = context.Categories.First(c => c.NormalizedName == "soups");
                var mains = context.Categories.First(c => c.NormalizedName == "main-dishes");

                var dishes = new List<Dish>
                {
                    new Dish { Name = "Borsch", Description = "Red soup", Calories = 300, Category = soups, Image = "borsch.jpg" },
                    new Dish { Name = "Chicken Soup", Description = "Chicken soup", Calories = 250, Category = soups, Image = "chicken_soup.jpg" },
                    new Dish { Name = "Steak", Description = "Grilled meat", Calories = 800, Category = mains, Image = "steak.jpg" }
                };
                await context.Dishes.AddRangeAsync(dishes);
                await context.SaveChangesAsync();
            }
        }
    }
}
