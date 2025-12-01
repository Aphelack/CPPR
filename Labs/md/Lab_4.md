# Кафедра информатики. Гламаздин И.И.

## Лабораторная работа №4

### Работа с REST API

---

## 1. Цель работы

Знакомство с принципом работы сервисов REST.

---

## 2. Задача работы

Научиться создавать REST API сервисы и взаимодействовать с ними из приложений.
**Время выполнения работы: 2 часа**

---

## 3. Выполнение работы

### 3.1. Создание проекта

1. Добавьте в решение проект **ASP.NET Core Web API**.
   Название проекта: `XXX.API`, где `XXX` – название вашего решения.

2. Добавьте в проект ссылку на проект `XXX.Domain`.

3. Используйте порты **7002** и **5002** — измените файл `launchSettings.json`.

4. Добавьте NuGet пакеты:

   * `Microsoft.EntityFrameworkCore`
   * `Microsoft.EntityFrameworkCore.Tools`

5. Добавьте папку **wwwroot/Images**. Скопируйте туда изображения из основного проекта.

6. В `Program.cs` добавьте Middleware статических файлов.

7. В настройках решения включите одновременный запуск проектов
   (в VS Code — запуск вручную).

8. Чтобы не открывался браузер при старте:
   Закомментируйте строку в `launchSettings.json`:

   ```json
   "launchBrowser": true,
   ```

---

### 3.2. Выбор БД

#### 3.2.1. Возможные варианты

Вы можете использовать любую реляционную СУБД:

* MS SqlServerExpress
* SQLite
* PostgreSQL
* MySQL

Примеры ниже — для PostgreSQL.

---

#### 3.2.2. SQLite

Добавьте NuGet пакет:

```
Microsoft.EntityFrameworkCore.Sqlite
```

---

#### 3.2.3. PostgreSQL

Добавьте NuGet пакет:

```
Aspire.Npgsql.EntityFrameworkCore.PostgreSQL
```

---

### 3.3. Установка сервера PostgreSQL

PostgreSQL можно установить:

* [https://www.postgresql.org/download/](https://www.postgresql.org/download/)
* pgAdmin4: [https://www.pgadmin.org/download/](https://www.pgadmin.org/download/)

#### Пример установки в Docker

Создайте файл **compose.yaml**:

```yaml
services:
  postgres:
    image: postgres:17.5-bookworm
    container_name: postgres
    env_file:
      - postgres.env
    environment:
      POSTGRES_USER: ${POSTGRES_USER}
      POSTGRES_PASSWORD: ${POSTGRES_PASSWORD}
      POSTGRES_DB: ${POSTGRES_DB}
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data
    networks:
      - postgres_network

  pgadmin:
    image: dpage/pgadmin4:9.4.0
    container_name: pgadmin4
    env_file:
      - postgres.env
    environment:
      PGADMIN_DEFAULT_EMAIL: ${PGADMIN_DEFAULT_EMAIL}
      PGADMIN_DEFAULT_PASSWORD: ${PGADMIN_DEFAULT_PASSWORD}
      PGADMIN_LISTEN_PORT: 5454
    ports:
      - "5454:5454"
    networks:
      - postgres_network
    depends_on:
      - postgres

volumes:
  postgres_data:
    driver: local

networks:
  postgres_network:
    driver: bridge
```

Файл **postgres.env**:

```
POSTGRES_USER=admin
POSTGRES_PASSWORD=123456
POSTGRES_DB=postgres
PGADMIN_DEFAULT_EMAIL=admin@example.com
PGADMIN_DEFAULT_PASSWORD=123456
```

Запуск контейнеров:

```
docker compose --env-file postgres.env up -d
```

---

### 3.4. Создание контекста БД

В проект `XXX.API`:

* создайте папку **Data**
* добавьте класс `AppDbContext`, принимающий:

```csharp
DbContextOptions<AppDbContext>
```

В классе создайте `DbSet<>` для сущностей из `XXX.Domain`.

---

### 3.5. Начальная миграция БД

В `appsettings.json` добавьте строку подключения:

```json
"ConnectionStrings": {
  "Postgres": "Server=127.0.0.1; Port=5432; Database=MenuDvb_V01; User Id=admin; Password=123456"
}
```

В `Program.cs` зарегистрируйте контекст:

```csharp
builder.Services.AddDbContext<AppDbContext>(
    options => options.UseNpgsql(connString));
```

Создайте миграцию:

```
dotnet ef migrations add Initial
dotnet ef database update
```

Проверьте в pgAdmin, что БД создана.

---

### 3.6. Заполнение базы начальными данными

Создайте класс **DbInitializer** в папке Data.

Метод:

```csharp
public static async Task SeedData(WebApplication app)
```

Рекомендации:

* получайте контекст через `app.Services.CreateScope()`
* выполняйте миграцию перед заполнением
* не задавайте Id вручную
* изображения хранятся в `/wwwroot/Images/`

Пример:

```csharp
using var scope = app.Services.CreateScope();
var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
await context.Database.MigrateAsync();
```

---

### 3.7. Создание контроллера API

Добавьте REST API контроллер:

**В Visual Studio:**

`Add → New Scaffolded Item → API Controller with actions`

**В VS Code:**

Установите пакеты:

```
dotnet add package Microsoft.VisualStudio.Web.CodeGeneration.Design -v 7.0.0
dotnet add package Microsoft.EntityFrameworkCore.Design -v 7.0.0
dotnet add package Microsoft.EntityFrameworkCore.SqlServer -v 7.0.0
dotnet tool install -g dotnet-aspnet-codegenerator
```

Создайте контроллер:

```
dotnet aspnet-codegenerator controller -name <Имя> -async -api -m <Модель> -dc <Контекст> -outDir Controllers
```

Убедитесь, что в `Program.cs` есть:

```csharp
builder.Services.AddControllers();
app.MapControllers();
```

---

### 3.8. Создание Minimal API

Создайте папку **EndPoints**.

Сгенерируйте endpoints:

`Add → New Scaffolded Item → API with read/write endpoints`

После генерации подключите в Program.cs:

```csharp
app.MapDishEndpoints();
```

---

### 3.9. Проверка API

Запустите проект.
Перейдите в браузере по URL API:

```
https://localhost:7002/api/<route>
```

Данные должны быть в формате JSON.

---

### 3.10. Доработка group.MapGet("/")

Задача:

* возвращать `ResponseData`
* добавить фильтрацию по категории
* добавить пагинацию
* ограничить размер страницы: `maxPageSize = 20`
* использовать MediatR

Команда для MediatR:

```
dotnet add package MediatR
```

Добавьте Use-Case:

`Use-Cases/GetListOfProducts.cs`

Пример:

```csharp
public sealed record GetListOfProducts(
    string? categoryNormalizedName,
    int pageNo = 1,
    int pageSize = 3)
    : IRequest<ResponseData<ListModel<Dish>>>;
```

Минимальная endpoint:

```csharp
group.MapGet("/{category:alpha?}",
    async (IMediator mediator, string? category, int pageNo = 1) =>
    {
        var data = await mediator.Send(new GetListOfProducts(category, pageNo));
        return TypedResults.Ok(data);
    })
.WithName("GetAllDishes")
.WithOpenApi();
```

---

### 3.11. Проверка API

Запустите проект
Проверьте выдачу JSON для всех API маршрутов.

---

### 3.12. Получение данных с API в проекте XXX.UI

#### 3.12.1. Подготовка проекта

Создайте класс:

```csharp
public class UriData
{
    public string ApiUri { get; set; } = string.Empty;
}
```

Добавьте в `appsettings.json`:

```json
"UriData": {
  "ApiUri": "https://localhost:7002/api/"
}
```

---

### 3.12.2. Создание сервисов ApiProductService и ApiCategoryService

Пример регистрации HttpClient:

```csharp
builder.Services.AddHttpClient<IProductService, ApiProductService>(opt =>
    opt.BaseAddress = new Uri(UriData.ApiUri + "dish"));

builder.Services.AddHttpClient<ICategoryService, ApiCategoryService>(opt =>
    opt.BaseAddress = new Uri(UriData.ApiUri + "categories"));
```

Пример чтения данных:

```csharp
var response = await _httpClient.GetAsync(new Uri(urlString.ToString()));
return await response.Content.ReadFromJsonAsync<ResponseData<ListModel<Dish>>>(_serializerOptions);
```

Пример записи:

```csharp
var response = await _httpClient.SendAsync(request);
```

---

## 4. Контрольные вопросы

1. Чем контроллер API отличается от обычного контроллера?
2. Как выбирается метод контроллера API при запросе?
3. В каком виде данные передаются в контроллер?
4. Как получить Scoped-сервис?
5. Что могут возвращать методы контроллера API?
6. Что такое Minimal API?
7. Как зарегистрировать конечную точку Minimal API?
8. Как зарегистрировать группу конечных точек Minimal API?