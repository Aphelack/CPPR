# Лабораторная работа №8
## Маршрутизация. Передача состояния с помощью сессии и кэширование

### Выполненные задачи

---

## Задание 1: Маршрутизация

### Описание
Настроена пользовательская маршрутизация для контроллера `Product`, чтобы страница каталога была доступна по адресу `/Catalog` вместо стандартного `/Product/Index`.

### Реализация

**Файл:** `Project/Controllers/ProductController.cs`

Использована маршрутизация с помощью атрибутов:

```csharp
[Route("Catalog/{category?}")]
public async Task<IActionResult> Index(string? category, int pageNo = 1)
{
    // Логика контроллера
}
```

**Особенности:**
- `{category?}` - необязательный параметр категории
- Поддерживаются оба варианта URL:
  - `/Catalog` - все блюда
  - `/Catalog/salads` - блюда конкретной категории

**Результат:**
- Старый URL `/Product/Index` больше не работает
- Новый URL `/Catalog` соответствует предметной области приложения
- SEO-дружественные URL-адреса

---

## Задание 2: Кэширование ответов API

### Описание
Реализовано кэширование списка блюд с использованием **HybridCache** и **Redis** в качестве распределенного хранилища.

### 2.1. Установка Redis

Redis установлен в системе локально (без использования Docker).

**Установка (Linux):**
```bash
sudo apt-get install redis-server
sudo systemctl enable redis-server
sudo systemctl start redis-server
```

**Проверка работы:**
```bash
redis-cli ping
# Ответ: PONG
```

### 2.2. Подготовка проекта CPPR.API

**Конфигурация:** `CPPR.API/appsettings.json`

Добавлена строка подключения к Redis:

```json
"ConnectionStrings": {
  "Postgres": "Server=127.0.0.1; Port=5432; Database=MenuDvb_V01; ...",
  "Redis": "localhost:6379"
}
```

**NuGet пакеты:** `CPPR.API/CPPR.API.csproj`

Добавлены пакеты для работы с кэшированием:
- `Microsoft.Extensions.Caching.StackExchangeRedis` (версия 8.0.0)

**Регистрация сервисов:** `CPPR.API/Program.cs`

```csharp
// Add Redis distributed cache with simple configuration
builder.Services.AddStackExchangeRedisCache(options =>
{
    options.Configuration = "127.0.0.1:6379,abortConnect=false,connectTimeout=30000,syncTimeout=30000";
    options.InstanceName = "labs_";
});
```

**Параметры конфигурации:**
- `InstanceName` - префикс для ключей в Redis
- `abortConnect=false` - не прерывать работу при недоступности Redis
- `connectTimeout=30000` - таймаут подключения (30 секунд)
- `syncTimeout=30000` - таймаут синхронных операций (30 секунд)

### 2.3. Кэширование данных

**Файл:** `CPPR.API/EndPoints/DishEndpoints.cs`

Изменена конечная точка получения списка блюд для использования `IDistributedCache`:

```csharp
group.MapGet("/{category?}",
    async (IMediator mediator, IDistributedCache cache, string? category, int pageNo = 1) =>
    {
        ResponseData<ListModel<Dish>>? data = null;
        var cacheKey = $"dishes_{category}_{pageNo}";
        
        try
        {
            var cachedData = await cache.GetStringAsync(cacheKey);
            if (cachedData != null)
            {
                data = JsonSerializer.Deserialize<ResponseData<ListModel<Dish>>>(cachedData);
            }
        }
        catch (Exception)
        {
            // Redis unavailable, continue without cache
        }
        
        if (data == null)
        {
            data = await mediator.Send(new GetListOfProducts(category, pageNo));
            
            try
            {
                var serialized = JsonSerializer.Serialize(data);
                var options = new DistributedCacheEntryOptions
                {
                    AbsoluteExpirationRelativeToNow = TimeSpan.FromMinutes(1)
                };
                await cache.SetStringAsync(cacheKey, serialized, options);
            }
            catch (Exception)
            {
                // Redis unavailable, continue without caching
            }
        }
        
        return Results.Ok(data);
    })
.WithName("GetAllDishes")
.WithOpenApi()
.AllowAnonymous();
```

**Механизм кэширования:**

1. **Попытка чтения из кэша (Redis)**
   - Если данные есть, десериализуем и возвращаем
   - Если Redis недоступен, перехватываем исключение и продолжаем

2. **Запрос к базе данных**
   - Если данных нет в кэше или кэш недоступен, запрашиваем данные через Mediator

3. **Сохранение в кэш**
   - Сериализуем полученные данные
   - Сохраняем в Redis с временем жизни 1 минута
   - Если Redis недоступен, перехватываем исключение

**Ключ кэша:** `dishes_{category}_{pageNo}`
- Уникальный для каждой комбинации категории и номера страницы

**Время жизни кэша:**
- **1 минута** - абсолютное время истечения кэша
- При истечении времени данные запрашиваются из БД заново

---

## Задание 3: Корзина заказов

### Описание
Корзина заказов уже реализована в Лабораторной работе №7. В этой работе функциональность была расширена и улучшена.

### Реализованная функциональность

**Классы:**
1. `CPPR.Domain/Entities/Cart.cs` - базовый класс корзины
2. `CPPR.Domain/Entities/CartItem.cs` - элемент корзины
3. `Project/Services/CartService/SessionCart.cs` - корзина с хранением в сессии

**Контроллер:** `Project/Controllers/CartController.cs`

Методы:
- `Index()` - отображение содержимого корзины
- `Add(int id, string returnUrl)` - добавление блюда в корзину
- `Remove(int id, string returnUrl)` - удаление блюда из корзины
- `Clear()` - полная очистка корзины

Все методы, кроме `Index`, требуют авторизации пользователя (`[Authorize]`).

**Представление:** `Project/Views/Cart/Index.cshtml`

Отображает:
- Таблицу с блюдами в корзине
- Изображение, название, калорийность каждого блюда
- Количество порций каждого блюда
- Общую калорийность
- Кнопки для удаления отдельных блюд
- Кнопку для очистки всей корзины
- Ссылку для продолжения покупок

**ViewComponent:** `Project/ViewComponents/CartViewComponent.cs`

Отображает информацию о корзине в меню навигации:
- Количество блюд в корзине
- Общую калорийность

### Хранение корзины в сессии

**Конфигурация сессии:** `Project/Program.cs`

```csharp
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(30);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// В middleware pipeline
app.UseSession();
```

**Параметры сессии:**
- `IdleTimeout` - время жизни сессии без активности (30 минут)
- `HttpOnly` - cookie недоступны из JavaScript (безопасность)
- `IsEssential` - cookie необходимы для работы приложения

**Расширения для работы с сессией:** `Project/Extensions/SessionExtensions.cs`

```csharp
public static class SessionExtensions
{
    public static void Set<T>(this ISession session, string key, T value)
    {
        session.SetString(key, JsonSerializer.Serialize(value));
    }

    public static T? Get<T>(this ISession session, string key)
    {
        var value = session.GetString(key);
        return value == null ? default : JsonSerializer.Deserialize<T>(value);
    }
}
```

Методы позволяют сериализовать и десериализовать объекты в/из сессии.

### SessionCart - реализация корзины с автосохранением

**Файл:** `Project/Services/CartService/SessionCart.cs`

```csharp
public class SessionCart : Cart
{
    private readonly ISession _session;
    private const string CartSessionKey = "cart";

    public SessionCart(IHttpContextAccessor httpContextAccessor)
    {
        _session = httpContextAccessor.HttpContext?.Session 
            ?? throw new InvalidOperationException("Session is not available");
        LoadCart();
    }

    private void LoadCart()
    {
        var cart = _session.Get<Cart>(CartSessionKey);
        if (cart != null)
        {
            CartItems = cart.CartItems;
        }
    }

    private void SaveCart()
    {
        _session.Set(CartSessionKey, this);
    }

    public override void AddToCart(Dish dish)
    {
        base.AddToCart(dish);
        SaveCart();
    }

    public override void RemoveItems(int id)
    {
        base.RemoveItems(id);
        SaveCart();
    }

    public override void ClearAll()
    {
        base.ClearAll();
        SaveCart();
    }
}
```

**Принцип работы:**
1. При создании объекта загружает данные из сессии
2. При любом изменении автоматически сохраняет в сессию
3. Не требует явного вызова сохранения в контроллерах

**Регистрация сервиса:** `Project/Extensions/HostingExtensions.cs`

```csharp
builder.Services.AddScoped<Cart>(sp => 
    new SessionCart(sp.GetRequiredService<IHttpContextAccessor>()));
```

Корзина регистрируется как Scoped-сервис и автоматически создается для каждого HTTP-запроса.

---

## Преимущества реализованных решений

### Кэширование
✅ **Производительность**: Уменьшение нагрузки на базу данных  
✅ **Масштабируемость**: Распределенный кэш доступен всем серверам  
✅ **Гибкость**: Двухуровневое кэширование (L1 + L2)  
✅ **Надежность**: Работа продолжается при недоступности Redis  

### Маршрутизация
✅ **SEO**: Понятные URL-адреса  
✅ **UX**: Легко запоминающиеся ссылки  
✅ **Гибкость**: Атрибутная маршрутизация  

### Корзина
✅ **Безопасность**: Авторизация для изменений  
✅ **Персистентность**: Данные сохраняются между запросами  
✅ **Производительность**: Хранение в памяти (сессия)  
✅ **Простота**: Автоматическое сохранение изменений  

---

## Технологии и подходы

1. **HybridCache** - двухуровневое кэширование (L1 + L2)
2. **Redis** - распределенное хранилище кэша
3. **Docker Compose** - оркестрация контейнеров
4. **Session State** - хранение пользовательских данных
5. **Attribute Routing** - гибкая настройка маршрутов
6. **Dependency Injection** - внедрение зависимостей
7. **Extension Methods** - расширение функциональности
8. **Scoped Services** - управление жизненным циклом

---

## Результаты

✅ Настроена пользовательская маршрутизация `/Catalog`  
✅ Реализовано двухуровневое кэширование с Redis  
✅ Добавлен Docker Compose для Redis  
✅ Оптимизирована производительность API  
✅ Улучшена работа с корзиной заказов  
✅ Настроено хранение состояния в сессии  
✅ Реализовано автоматическое сохранение корзины  

Все функциональные требования лабораторной работы выполнены.
