Кафедра информатики. Гламаздин И.И.

# Лабораторная работа №3

## Работа с данными

### 1. Цель работы

Изучение механизмов обмена данными между контроллером и представлением.

### 2. Задача работы

Научиться подготавливать данные в контроллере и передавать их представлению для отображения.
**Время выполнения работы:** 2 часа

---

## 3. Выполнение работы

Используйте проект из лабораторной работы №2.

### 3.1. Описание предметной области

Выберите любую предметную область.
Добавьте в решение новый проект – библиотеку классов .Net. Имя проекта: **XXX.Domain**, где XXX – имя вашего решения.

В созданном проекте создайте папку **Entities**.

Для одной сущности из выбранной предметной области создайте класс со свойствами:

* ID – уникальный номер
* Название
* Описание
* Категория
* Цена/Вес/Расстояние – любой параметр для дальнейшей математической обработки
* Изображение – путь к файлу
* Mime тип изображения

Создайте также класс Category.
Отношение: **один-ко-многим**.

В проекте **XXX.UI** сделайте ссылку на библиотеку классов.
В файл `_ViewImports.cshtml` добавьте:

```
@using XXX.Domain.Entities
```

Добавьте файл **GlobalUsings.cs** и подключите глобальное пространство имён:

```
global using XXX.Domain.Entities;
```

---

### 3.2. Вспомогательные классы

В проект **XXX.Domain** добавьте папку **Models**.

Создайте класс **ResponseData**:

```csharp
public class ResponseData<T>
{
    public T? Data { get; set; }
    public bool Successfull { get; set; } = true;
    public string? ErrorMessage { get; set; }

    public static ResponseData<T> Success(T data)
        => new ResponseData<T> { Data = data };

    public static ResponseData<T> Error(string message, T? data = default)
        => new ResponseData<T> { ErrorMessage = message, Successfull = false, Data = data };
}
```

Создайте класс **ListModel**:

```csharp
public class ListModel<T>
{
    public List<T> Items { get; set; } = new();
    public int CurrentPage { get; set; } = 1;
    public int TotalPages { get; set; } = 1;
}
```

В **GlobalUsings.cs** добавьте:

```
global using XXX.Domain.Models;
```

---

### 3.3. Подготовка для регистрации пользовательских сервисов

В проект **XXX.UI** создайте папку **Extensions** и файл:

```csharp
public static class HostingExtensions
{
    public static void RegisterCustomServices(this WebApplicationBuilder builder)
    {
    }
}
```

В `Program.cs` добавьте:

```
builder.RegisterCustomServices();
```

---

### 3.4. Описание сервисов

В проект **XXX.UI** добавьте папку **Services**.

#### Сервис категорий

Создайте папку **CategoryService**, интерфейс:

```csharp
public interface ICategoryService
{
    Task<ResponseData<List<Category>>> GetCategoryListAsync();
}
```

Создайте класс **MemoryCategoryService**:

```csharp
public class MemoryCategoryService : ICategoryService
{
    public Task<ResponseData<List<Category>>> GetCategoryListAsync()
    {
        var categories = new List<Category>
        {
            new Category {Id=1, Name="Стартеры", NormalizedName="starters"},
            new Category {Id=2, Name="Салаты", NormalizedName="salads"},
            new Category {Id=3, Name="Супы", NormalizedName="soups"},
            new Category {Id=4, Name="Основные блюда", NormalizedName="main-dishes"},
            new Category {Id=5, Name="Напитки", NormalizedName="drinks"},
            new Category {Id=6, Name="Десерты", NormalizedName="desserts"}
        };

        return Task.FromResult(ResponseData<List<Category>>.Success(categories));
    }
}
```

Зарегистрируйте как scoped-сервис.

Добавьте в GlobalUsings:

```
global using XXX.UI.Services.CategoryService;
```

#### Сервис продуктов

Создайте папку **ProductService**, интерфейс:

```csharp
public interface IProductService
{
    Task<ResponseData<ListModel<Dish>>> GetProductListAsync(string? categoryNormalizedName, int pageNo=1);
    Task<ResponseData<Dish>> GetProductByIdAsync(int id);
    Task UpdateProductAsync(int id, Dish product, IFormFile? formFile);
    Task DeleteProductAsync(int id);
    Task<ResponseData<Dish>> CreateProductAsync(Dish product, IFormFile? formFile);
}
```

Создайте **MemoryProductService** и заполните коллекции в конструкторе.

Пример:

```csharp
private void SetupData()
{
    _dishes = new List<Dish>
    {
        new Dish { Id = 1, Name="Суп-харчо", Description="Очень острый, невкусный", Calories=200,
            Image="../Images/Суп.jpg",
            Category=_categories.Find(c=>c.NormalizedName=="soups") },

        new Dish { Id = 2, Name="Борщ", Description="Много сала, без сметаны", Calories=330,
            Image="../Images/Борщ.jpg",
            Category=_categories.Find(c=>c.NormalizedName=="soups") }
    };
}
```

Зарегистрируйте сервис.
Добавьте в GlobalUsings:

```
global using XXX.UI.Services.ProductService;
```

---

### 3.5. Вывод списка объектов

В папке **Controllers** создайте **ProductController**.

Пример метода:

```csharp
public async Task<IActionResult> Index()
{
    var productResponse = await _service.GetProductListAsync(null);
    if (!productResponse.Successfull)
        return NotFound(productResponse.ErrorMessage);

    return View(productResponse.Data.Items);
}
```

Сгенерируйте представление **Index** (шаблон List).
Замените вывод изображения:

```
<img src="@item.Image"/>
```

---

### 3.6. Оформление списка объектов

Оформите список объектов в виде **Bootstrap Cards**, 3 в ряду.

Каждая карта содержит:

* изображение
* название
* описание
* кнопку «Добавить в корзину»

Получение returnUrl:

```csharp
var request = ViewContext.HttpContext.Request;
var returnUrl = request.Path + request.QueryString.ToUriComponent();
```

---

### 3.7. Фильтр по категориям

Метод контроллера:

```csharp
public async Task<IActionResult> Index(string? category)
```

Передайте категории через ViewBag/ViewData.

Пример разметки:

```html
<li>
    <a class="dropdown-item"
       asp-controller="product"
       asp-route-category="@item.NormalizedName">
       @item.Name
    </a>
</li>
```

Фильтрация:

```csharp
var data = _dishes
    .Where(d => categoryNormalizedName == null ||
           d.Category.NormalizedName == categoryNormalizedName)
    .ToList();
```

---

### 3.8. Разбиение на страницы

Размер страницы укажите в `appsettings.json`:

```json
"ItemsPerPage": 3
```

Метод контроллера:

```csharp
public async Task<IActionResult> Index(string? category, int pageNo = 1)
```

В `MemoryProductService` используйте:

* Take
* Skip
* вычисление TotalPages

Модель представления:

```
@model ListModel<Dish>
```

---

### 3.9. Кнопки переключения страниц

Используйте Bootstrap Pagination.

Пример:

```csharp
int prev = Model.CurrentPage == 1 ? 1 : Model.CurrentPage - 1;
int next = Model.CurrentPage == Model.TotalPages ? Model.TotalPages : Model.CurrentPage + 1;
```

Получение категории:

```csharp
string? category = request.Query["category"]?.ToString();
```

---

## 4. Контрольные вопросы

1. Как зарегистрировать сервис в ASP.Net Core?
2. Чем отличается Transient сервис от Scoped сервиса?
3. Как внедрить сервис в метод контроллера?
4. Как передать данные от клиента в метод контроллера?
5. Где механизм привязки (Model binding) ищет нужные значения?
6. Как получить данные из файла appsettings.json?
7. Как передать данные с помощью IOptions?
8. Как в коде прочитать значение строки запроса?
9. Как в теге `<a>` передать данные через tag-helper?
10. Что такое «Явное выражение Razor»?
