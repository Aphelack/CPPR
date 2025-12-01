# Лабораторная работа №4: Работа с REST API

## Цель работы
Знакомство с принципом работы сервисов REST и создание API для приложения.

## Выполнение работы

### 1. Создание проекта API
Был создан проект `CPPR.API` типа ASP.NET Core Web API.
Добавлены ссылки на проект `CPPR.Domain`.
Настроены порты в `launchSettings.json`.

### 2. Настройка базы данных (PostgreSQL)
Вместо Docker контейнера была использована системная установка PostgreSQL.
Строка подключения в `appsettings.json`:
```json
"ConnectionStrings": {
  "Postgres": "Server=127.0.0.1; Port=5432; Database=MenuDvb_V01; User Id=admin; Password=123456"
}
```
Создан контекст `AppDbContext` и выполнены миграции:
```bash
dotnet ef migrations add Initial
dotnet ef database update
```

### 3. Инициализация данных
Создан класс `DbInitializer` для заполнения базы начальными данными (категории и блюда).
Инициализация добавлена в `Program.cs`:
```csharp
await DbInitializer.SeedData(app);
```

### 4. Реализация API

#### 4.1. Контроллер категорий
Создан `CategoriesController` для получения списка категорий.
```csharp
[Route("api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly AppDbContext _context;

    public CategoriesController(AppDbContext context)
    {
        _context = context;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Category>>> GetCategories()
    {
        return await _context.Categories.ToListAsync();
    }
}
```

#### 4.2. Minimal API для блюд
Реализован Minimal API для получения списка блюд с фильтрацией и пагинацией.
Использован паттерн CQRS с библиотекой MediatR.

**Endpoint (DishEndpoints.cs):**
```csharp
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
```

**Use-Case (GetListOfProducts.cs):**
Обработчик `GetListOfProductsHandler` реализует логику фильтрации по категории и пагинации.
```csharp
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
```

### 5. Результат
API успешно запущено и готово к использованию клиентом (UI).
Docker окружение было очищено, так как используется локальный PostgreSQL.
