using CPPR.API.Data;
using CPPR.Domain.Entities;
using CPPR.Domain.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CPPR.API.Use_Cases
{
    public sealed record GetListOfProducts(
        string? categoryNormalizedName,
        int pageNo = 1,
        int pageSize = 3)
        : IRequest<ResponseData<ListModel<Dish>>>;

    public class GetListOfProductsHandler : IRequestHandler<GetListOfProducts, ResponseData<ListModel<Dish>>>
    {
        private readonly AppDbContext _context;

        public GetListOfProductsHandler(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ResponseData<ListModel<Dish>>> Handle(GetListOfProducts request, CancellationToken cancellationToken)
        {
            var query = _context.Dishes.Include(d => d.Category).AsQueryable();

            if (!string.IsNullOrEmpty(request.categoryNormalizedName))
            {
                query = query.Where(d => d.Category!.NormalizedName.Equals(request.categoryNormalizedName));
            }

            var totalItems = await query.CountAsync(cancellationToken);
            var totalPages = (int)Math.Ceiling(totalItems / (double)request.pageSize);

            if (request.pageNo > totalPages && totalPages > 0)
                return ResponseData<ListModel<Dish>>.Error("No such page");

            var items = await query
                .Skip((request.pageNo - 1) * request.pageSize)
                .Take(request.pageSize)
                .ToListAsync(cancellationToken);

            var listModel = new ListModel<Dish>
            {
                Items = items,
                CurrentPage = request.pageNo,
                TotalPages = totalPages
            };

            return ResponseData<ListModel<Dish>>.Success(listModel);
        }
    }
}
