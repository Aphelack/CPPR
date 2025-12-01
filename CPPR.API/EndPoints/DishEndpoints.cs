using CPPR.API.Use_Cases;
using CPPR.Domain.Entities;
using CPPR.Domain.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CPPR.API.EndPoints
{
    public static class DishEndpoints
    {
        public static void MapDishEndpoints(this IEndpointRouteBuilder routes)
        {
            var group = routes.MapGroup("/api/dish").WithTags("Dish");

            group.MapGet("/{category:alpha?}",
                async (IMediator mediator, string? category, int pageNo = 1) =>
                {
                    var data = await mediator.Send(new GetListOfProducts(category, pageNo));
                    return Results.Ok(data);
                })
            .WithName("GetAllDishes")
            .WithOpenApi();
        }
    }
}
