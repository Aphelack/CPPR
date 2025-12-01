using System.Text.Json;
using CPPR.API.Data;
using CPPR.API.Use_Cases;
using CPPR.Domain.Entities;
using CPPR.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CPPR.API.EndPoints
{
    public static class DishEndpoints
    {
        public static void MapDishEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/dish")
                .WithTags("Dish")
                .DisableAntiforgery()
                .RequireAuthorization("admin");

            group.MapGet("/{category?}",
                async (IMediator mediator, string? category, int pageNo = 1) =>
                {
                    var data = await mediator.Send(new GetListOfProducts(category, pageNo));
                    return Results.Ok(data);
                })
            .WithName("GetAllDishes")
            .WithOpenApi()
            .AllowAnonymous();

            group.MapGet("/{id:int}", async (int id, AppDbContext db) =>
            {
                var dish = await db.Dishes.FindAsync(id);
                return dish is not null
                    ? Results.Ok(ResponseData<Dish>.Success(dish))
                    : Results.NotFound(ResponseData<Dish>.Error("Dish not found"));
            })
            .WithName("GetDishById")
            .WithOpenApi()
            .AllowAnonymous();

            group.MapPost("/", async (
                [FromForm] string dish,
                [FromForm] IFormFile? file,
                AppDbContext db,
                IMediator mediator
            ) =>
            {
                var newDish = JsonSerializer.Deserialize<Dish>(dish, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (newDish == null) return Results.BadRequest(ResponseData<Dish>.Error("Invalid dish data"));

                if (file != null)
                {
                    var imageUrl = await mediator.Send(new SaveImage(file));
                    newDish.Image = imageUrl;
                }
                
                if (newDish.Category != null)
                {
                    db.Entry(newDish.Category).State = EntityState.Unchanged;
                }

                db.Dishes.Add(newDish);
                await db.SaveChangesAsync();

                return Results.Created($"/api/dish/{newDish.Id}", ResponseData<Dish>.Success(newDish));
            })
            .WithName("CreateDish")
            .WithOpenApi();

            group.MapPut("/{id:int}", async (
                int id,
                [FromForm] string dish,
                [FromForm] IFormFile? file,
                AppDbContext db,
                IMediator mediator
            ) =>
            {
                var dishToUpdate = JsonSerializer.Deserialize<Dish>(dish, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                if (dishToUpdate == null || dishToUpdate.Id != id) return Results.BadRequest(ResponseData<Dish>.Error("Invalid dish data"));

                var existingDish = await db.Dishes.FindAsync(id);
                if (existingDish == null) return Results.NotFound(ResponseData<Dish>.Error("Dish not found"));

                if (file != null)
                {
                    if (!string.IsNullOrEmpty(existingDish.Image))
                    {
                        await mediator.Send(new DeleteImage(existingDish.Image));
                    }
                    var imageUrl = await mediator.Send(new SaveImage(file));
                    existingDish.Image = imageUrl;
                }

                existingDish.Name = dishToUpdate.Name;
                existingDish.Description = dishToUpdate.Description;
                existingDish.Calories = dishToUpdate.Calories;
                existingDish.CategoryId = dishToUpdate.CategoryId;

                await db.SaveChangesAsync();

                return Results.Ok(ResponseData<Dish>.Success(existingDish));
            })
            .WithName("UpdateDish")
            .WithOpenApi();

            group.MapDelete("/{id:int}", async (int id, AppDbContext db, IMediator mediator) =>
            {
                var dish = await db.Dishes.FindAsync(id);
                if (dish == null) return Results.NotFound(ResponseData<Dish>.Error("Dish not found"));

                if (!string.IsNullOrEmpty(dish.Image))
                {
                    await mediator.Send(new DeleteImage(dish.Image));
                }

                db.Dishes.Remove(dish);
                await db.SaveChangesAsync();

                return Results.Ok(ResponseData<string>.Success("Dish deleted"));
            })
            .WithName("DeleteDish")
            .WithOpenApi();
        }
    }
}
