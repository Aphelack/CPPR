using CPPR.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CPPR.API.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
    {
    }

    public DbSet<Dish> Dishes { get; set; }
    public DbSet<Category> Categories { get; set; }
}
