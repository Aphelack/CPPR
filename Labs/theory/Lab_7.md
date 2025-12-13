# Лабораторная работа №7
## Вспомогательные классы тэгов, Ajax

### Выполненные задачи

---

## Задание 1: Создание тэг-хелпера Pager

### Описание
Создан пользовательский тэг-хелпер `PagerTagHelper` для генерации кнопок пагинации с использованием класса `TagBuilder`.

### Реализация

**Файл:** `Project/TagHelpers/PagerTagHelper.cs`

Тэг-хелпер поддерживает следующие атрибуты:
- `current-page` - текущая страница
- `total-pages` - общее количество страниц
- `category` - категория для фильтрации (опционально)
- `admin` - флаг для использования на административных страницах

**Особенности реализации:**
1. Использует `LinkGenerator` для генерации URL-адресов
2. Поддерживает два режима:
   - Обычные страницы: использует `GetPathByAction` для контроллеров
   - Административные страницы: генерирует URL с query-параметром `pageNo`
3. Применяет Bootstrap-классы для стилизации (`pagination`, `page-item`, `page-link`)
4. Автоматически отключает кнопки "Previous" на первой странице и "Next" на последней

**Регистрация:**
Тэг-хелпер зарегистрирован в `Views/_ViewImports.cshtml` и `Areas/Admin/Pages/_ViewImports.cshtml`:
```cshtml
@addTagHelper *, Project
```

**Использование:**
```cshtml
<!-- Для обычных страниц -->
<pager current-page="@Model.CurrentPage"
       total-pages="@Model.TotalPages"
       category="@currentCategory"></pager>

<!-- Для административных страниц -->
<pager current-page="@Model.CurrentPage"
       total-pages="@Model.TotalPages"
       admin="true"></pager>
```

---

## Задание 2: Ajax-пагинация

### Описание
Реализована асинхронная загрузка страниц без перезагрузки всей страницы с использованием технологии Ajax.

### Реализация

**1. Частичное представление:** `Project/Views/Product/_ListPartial.cshtml`

Создано частичное представление, содержащее:
- Список товаров (карточки с изображениями, описанием, калориями)
- Пейджер для навигации

**2. Обновление основного представления:** `Project/Views/Product/Index.cshtml`

Основное представление теперь содержит контейнер `#product-list`, в который загружается частичное представление:
```cshtml
<div id="product-list">
    @await Html.PartialAsync("_ListPartial", Model)
</div>
```

**3. JavaScript для Ajax-запросов:** `Project/wwwroot/js/site.js`

```javascript
$(document).ready(function() {
    // Применяем Ajax только если существует #product-list (не на admin-страницах)
    if ($('#product-list').length > 0) {
        $(document).on('click', '.page-link', function(e) {
            e.preventDefault();
            var url = $(this).attr('href');
            
            if (url && url !== '#') {
                $.ajax({
                    url: url,
                    type: 'GET',
                    headers: {
                        'X-Requested-With': 'XMLHttpRequest'
                    },
                    success: function(data) {
                        $('#product-list').html(data);
                    }
                });
            }
        });
    }
});
```

**Особенности:**
- Ajax работает только на страницах с контейнером `#product-list`
- На административных страницах используется обычная навигация
- Устанавливается заголовок `X-Requested-With: XMLHttpRequest`

**4. Обработка на сервере:** `Project/Controllers/ProductController.cs`

Контроллер проверяет, является ли запрос асинхронным, и возвращает соответствующее представление:
```csharp
if (Request.IsAjaxRequest())
    return PartialView("_ListPartial", productResponse.Data);

return View(productResponse.Data);
```

---

## Задание 3: Расширяющий метод IsAjaxRequest

### Описание
Создан расширяющий метод для класса `HttpRequest` для проверки асинхронных запросов.

### Реализация

**Файл:** `Project/Extensions/HttpRequestExtensions.cs`

```csharp
public static class HttpRequestExtensions
{
    public static bool IsAjaxRequest(this HttpRequest request)
    {
        if (request == null)
            throw new ArgumentNullException(nameof(request));

        return request.Headers["X-Requested-With"] == "XMLHttpRequest";
    }
}
```

Метод проверяет наличие заголовка `X-Requested-With` со значением `XMLHttpRequest`, который автоматически устанавливается при Ajax-запросах.

---

## Дополнительные улучшения

### 1. Маршрутизация с атрибутами
В контроллере `ProductController` добавлена маршрутизация:
```csharp
[Route("Catalog/{category?}")]
public async Task<IActionResult> Index(string? category, int pageNo = 1)
```

Теперь страница каталога доступна по адресу `/Catalog` или `/Catalog/категория`.

### 2. Корзина заказов

**Созданные классы:**
- `CPPR.Domain/Entities/Cart.cs` - класс корзины
- `CPPR.Domain/Entities/CartItem.cs` - элемент корзины
- `Project/Services/CartService/SessionCart.cs` - сервис корзины с хранением в сессии
- `Project/Extensions/SessionExtensions.cs` - методы расширения для работы с сессией

**Функциональность Cart:**
- Добавление блюд в корзину
- Удаление блюд из корзины
- Подсчет общего количества блюд
- Подсчет общей калорийности
- Хранение в сессии пользователя

**Контроллер:** `Project/Controllers/CartController.cs`

Методы:
- `Index` - отображение корзины
- `Add` - добавление блюда (требует авторизации)
- `Remove` - удаление блюда (требует авторизации)
- `Clear` - очистка корзины (требует авторизации)

**ViewComponent:** `Project/ViewComponents/CartViewComponent.cs`

Отображает информацию о корзине в меню (количество товаров и общая калорийность).

**Представления:**
- `Views/Cart/Index.cshtml` - страница корзины с таблицей товаров
- `Views/Shared/Components/Cart/Default.cshtml` - компонент для меню

### 3. Настройка сессий

В `Program.cs` добавлена поддержка сессий:
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

### 4. Регистрация сервиса корзины

В `Extensions/HostingExtensions.cs`:
```csharp
builder.Services.AddScoped<Cart>(sp => 
    new SessionCart(sp.GetRequiredService<IHttpContextAccessor>()));
```

Корзина регистрируется как Scoped-сервис и автоматически связывается с сессией пользователя.

---

## Исправленные проблемы

### 1. Проблема с созданием блюд в API

**Проблема:** При создании блюда через административную панель возникала ошибка 403 Forbidden.

**Причина:** 
- API не извлекал роли из токена Keycloak корректно
- Роли в Keycloak хранятся в структуре `realm_access.roles`, которая не обрабатывалась автоматически

**Решение:** В `CPPR.API/Program.cs` добавлена обработка токена:
```csharp
options.Events = new JwtBearerEvents
{
    OnTokenValidated = context =>
    {
        var identity = context.Principal?.Identity as ClaimsIdentity;
        var realmAccessClaim = context.Principal?.Claims
            .FirstOrDefault(c => c.Type == "realm_access");
        
        if (realmAccessClaim != null)
        {
            var realmAccess = JsonDocument.Parse(realmAccessClaim.Value);
            if (realmAccess.RootElement.TryGetProperty("roles", out var roles))
            {
                foreach (var role in roles.EnumerateArray())
                {
                    identity.AddClaim(new Claim("role", role.GetString()));
                }
            }
        }
        return Task.CompletedTask;
    }
};
```

### 2. Проблема с сериализацией Dish

**Проблема:** При отправке объекта `Dish` на API включалась навигационная свойство `Category`, что вызывало ошибки.

**Решение:** В `ApiProductService` используется анонимный объект только с необходимыми полями:
```csharp
var dishData = new
{
    product.Id,
    product.Name,
    product.Description,
    product.Calories,
    product.CategoryId
};
```

### 3. Проблема с авторизацией HTTP-запросов

**Проблема:** Токен не передавался в запросах к API.

**Решение:** Токен устанавливается непосредственно в заголовки каждого HTTP-запроса:
```csharp
var token = await _tokenAccessor.GetAccessTokenAsync();
if (!string.IsNullOrEmpty(token))
{
    request.Headers.Authorization = 
        new AuthenticationHeaderValue("Bearer", token);
}
```

---

## Технологии и подходы

1. **Tag Helpers** - для создания переиспользуемых компонентов разметки
2. **Ajax (jQuery)** - для асинхронной загрузки данных
3. **Partial Views** - для модульной организации представлений
4. **Extension Methods** - для расширения функциональности стандартных классов
5. **Session** - для хранения состояния между запросами
6. **Dependency Injection** - для управления зависимостями (Cart, Services)
7. **View Components** - для отображения динамических компонентов (корзина в меню)
8. **Route Attributes** - для настройки маршрутизации
9. **JWT Bearer Authentication** - для защиты API-endpoints

---

## Результаты

✅ Создан переиспользуемый тэг-хелпер для пагинации  
✅ Реализована Ajax-пагинация без перезагрузки страницы  
✅ Создан расширяющий метод для определения Ajax-запросов  
✅ Реализована корзина заказов с хранением в сессии  
✅ Настроена маршрутизация через атрибуты  
✅ Исправлены проблемы с авторизацией и аутентификацией  
✅ Улучшена работа с API (сериализация, передача токенов)  

Все функциональные требования лабораторной работы выполнены.
