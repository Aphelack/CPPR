Готово! Вот ваш текст, **полностью преобразованный в Markdown**, без изменений смысла.

---

# Кафедра информатики. Гламаздин И.И.

# Лабораторная работа №5

## Razor Pages. Передача файлов

---

## 1. Цель работы

Знакомство со сценариями построения веб-приложений, основанных на страницах.
Знакомство с механизмом передачи файлов от клиента на сервер.

---

## 2. Задача работы

* Научиться создавать страницы Razor.
* Научиться передавать файлы на сервер.
* Научиться передавать файлы в REST–сервис.

**Время выполнения работы:** 2 часа

---

## 3. Выполнение работы. Страницы администратора

### 3.1. Постановка задачи

В проекте **XXX.UI** требуется создать страницы администратора, позволяющие манипулировать данными объектов вашей предметной области в базе данных
(в рамках лабораторных работ страницы для групп (категорий) объектов создавать не нужно).

Страницы должны быть выполнены по сценарию **Razor Pages**.

Страницы администратора должны располагаться в области (**Areas**) с названием **Admin** (см. п. 3.1 лабораторной работы №2).

---

### 3.2. Подготовка проекта

Чтобы вручную не создавать страницы администратора, предлагается воспользоваться механизмом **Scaffold**.

Однако данный механизм предполагает использование контекста базы данных.
Чтобы обойти это ограничение, предлагается в проекте **XXX.UI** *временно* описать контекст базы данных с нужными сущностями.

Установите в проекте **XXX.UI** пакеты NuGet:

* `Microsoft.EntityFrameworkCore`
* пакет любого провайдера, например `Microsoft.EntityFrameworkCore.Sqlite`

Опишите контекст БД:

```csharp
public class TempDbContext : DbContext
{
    public DbSet<Dish> Dishes { get; set; }

    protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
    {
        base.OnConfiguring(optionsBuilder);
        optionsBuilder.UseSqlite("");
    }
}
```

После создания страниц администратора контекст БД можно удалить из проекта.

---

### 3.3. Создание страниц администратора

Сгенерируйте страницы **Razor Pages** для выполнения CRUD-операций над объектами предметной области согласно п. 3.1.

В коде моделей **всех страниц** замените использование контекста БД на использование **IProductService**.

Для использования страницы макета и базовых пространств имён
в папку, где находятся страницы администратора, скопируйте файлы:

* `_ViewImports.cshtml`
* `_ViewStart.cshtml`

В разметке страницы **Index** вместо имени файла изображения укажите тэг для отображения изображения.

На странице **Index** оформите ссылки переключения между страницами с помощью иконок и стилей **Bootstrap**.

В классе **Program** зарегистрируйте использование страниц Razor:

```csharp
builder.Services.AddRazorPages();
app.MapRazorPages();
```

Запустите проект. Перейдите на страницу администрирования.
Убедитесь, что страницы работают корректно.

---

## 4. Выполнение работы. Передача файлов

### 4.1. Постановка задачи

Страницы создания и редактирования объекта должны предоставлять возможность передачи изображения объекта.

Изображения должны сохраняться в проекте **XXX.API**, в папке:

```
wwwroot/Images
```

Чтобы исключить дублирование имён, файлам назначаются **случайные имена**.

При удалении объекта или при редактировании (замене изображения) предыдущий файл должен удаляться.

Соглашение: имя поля файла при передаче через HTTP используется **file**.

---

### 4.2. Use-case сохранения файла (проект XXX.API)

В папку **Use-Cases** добавьте файл `SaveImage.cs`:

```csharp
public sealed record SaveImage(IFormFile file) : IRequest<string>;

public class SaveImageHandler(
    IWebHostEnvironment env,
    IHttpContextAccessor httpContextAccessor
) : IRequestHandler<SaveImage, string>
{
    public Task<string> Handle(SaveImage request, CancellationToken cancellationToken)
    {
        ...;
    }
}
```

* `IWebHostEnvironment` — для получения пути к *wwwroot*
* `IHttpContextAccessor` — для получения объекта `HttpContext`,
  чтобы сформировать корректный URL сохранённого изображения.

Запрос должен вернуть **URL файла**.

---

### 4.3. Конечная точка API для сохранения объекта с файлом

Для доступа клиента к файлам добавьте в **Program** проекта **XXX.API** middleware:

```csharp
app.UseStaticFiles();
// или
app.MapStaticAssets();
```

Для передачи файла данные нужно передавать в виде **MultipartFormData**.

Чтобы отменить Antiforgery-токен:

```csharp
var group = routes.MapGroup("/api/Dish")
    .WithTags(nameof(Dish))
    .DisableAntiforgery();
```

Пример конечной точки:

```csharp
group.MapPost("/", async (
    [FromForm] string dish,
    [FromForm] IFormFile? file,
    AppDbContext db,
    IMediator mediator
) =>
{
    var newDish = JsonSerializer.Deserialize<Dish>(dish);

    if (file != null)
        newDish.Image = await mediator.Send(new SaveImage(file));

    db.Dishes.Add(newDish);
    await db.SaveChangesAsync();

    return TypedResults.Created($"/api/Dish/{newDish.Id}", newDish);
})
.WithName("CreateDish")
.WithOpenApi();
```

---

### 4.4. Доработка сервиса ApiProductService (проект XXX.UI)

Добавьте в методы создание/редактирования возможность передачи файла.

Пример:

```csharp
public async Task<ResponseData<Dish>> CreateProductAsync(
    Dish product,
    IFormFile? formFile)
{
    product.Image = "Images/noimage.jpg";

    var request = new HttpRequestMessage
    {
        Method = HttpMethod.Post,
        RequestUri = _httpClient.BaseAddress
    };

    var content = new MultipartFormDataContent();

    if (formFile != null)
    {
        var streamContent = new StreamContent(formFile.OpenReadStream());
        content.Add(streamContent, "file", formFile.FileName);
    }

    var data = new StringContent(JsonSerializer.Serialize(product));
    content.Add(data, "dish");

    request.Content = content;

    var response = await _httpClient.SendAsync(request);

    if (response.IsSuccessStatusCode)
    {
        var responseData = await response.Content.ReadFromJsonAsync<ResponseData<Dish>>();
        return responseData;
    }

    _logger.LogError($"-----> object not created. Error: {response.StatusCode}");

    return ResponseData<Dish>.Error($"Объект не добавлен. Error: {response.StatusCode}");
}
```

---

### 4.5. Доработка страниц Create и Update

В модели страниц добавьте:

```csharp
[BindProperty]
public IFormFile? Image { get; set; }
```

Передайте `Image` в сервис:

```csharp
await _service.CreateProduct(Dish, Image);
```

В разметке добавьте input типа `file`:

```html
<form method="post" enctype="multipart/form-data">
    <input asp-for="Image" type="file" class="form-control" />
</form>
```

При использовании файлов необходим атрибут:

```
enctype="multipart/form-data"
```

---

### 4.5.1. Оформление страницы редактирования (необязательно)

При редактировании необходимо **выводить текущее изображение объекта**.

---

## 5. Контрольные вопросы

1. Чем сценарий Razor Pages отличается от MVC?
2. Что такое модель страницы Razor?
3. Как обрабатываются запросы к страницам Razor?
4. Как в разметке Razor получить доступ к свойствам модели?
5. Как привязать данные формы к модели страницы?
6. Для чего используется директива `@page`?
7. Какой URL будет у страницы `Register` на рисунке?
8. Чем отличается адрес `/index` от `./index`?
9. Как указать дополнительный сегмент маршрута (например, id)?
10. Какой middleware используется для передачи статического контента?
11. Какой интерфейс реализует объект файла, полученный сервером?
12. Что такое FileProvider?
13. Как получить путь к папке `wwwroot`?
14. В каком виде файл будет представлен на сервере?
15. Какой атрибут должен быть установлен в `<form>` для передачи файлов?

---

Если хотите — могу также сделать **красиво оформленный PDF**, **сокращённый конспект**, или **ответы на контрольные вопросы**.
