# Лабораторная работа №9
## Модульное тестирование

### Выполненные задачи

---

## Задание 1: Создание проекта тестов

### Описание
Создан проект `CPPR.Tests` для модульного тестирования приложения.

### Реализация

**Тип проекта:** xUnit Test Project
**Целевая платформа:** .NET 8.0
**SDK:** Microsoft.NET.Sdk.Web (для доступа к типам ASP.NET Core MVC)

**Установленные пакеты:**
- `xunit` - фреймворк тестирования
- `xunit.runner.visualstudio` - раннер тестов
- `NSubstitute` - библиотека для создания мок-объектов (имитаций)
- `Microsoft.EntityFrameworkCore.Sqlite` - провайдер БД для тестов in-memory

**Ссылки на проекты:**
- `CPPR.API`
- `CPPR.Domain`
- `Project` (UI)

---

## Задание 2: Тестирование контроллера Product

### Описание
Разработаны модульные тесты для метода `Index` контроллера `ProductController` в проекте UI.

### Реализация

**Файл:** `CPPR.Tests/ProductControllerTests.cs`

**Проверяемые сценарии:**
1. **Index_ReturnsNotFound_WhenCategoriesFail**
   - Проверяет, что метод возвращает 404, если сервис категорий вернул ошибку.
   - Имитация: `_categoryService.GetCategoryListAsync()` возвращает ошибку.

2. **Index_ReturnsNotFound_WhenProductsFail**
   - Проверяет, что метод возвращает 404, если сервис продуктов вернул ошибку.
   - Имитация: `_productService.GetProductListAsync()` возвращает ошибку.

3. **Index_ReturnsView_WithCorrectData_WhenSuccess**
   - Проверяет успешный сценарий.
   - Убеждается, что возвращается `ViewResult`.
   - Проверяет, что в модель представления переданы правильные данные (список блюд).
   - Проверяет, что во `ViewData` переданы список категорий и текущая категория.

4. **Index_SetsCurrentCategoryToAll_WhenCategoryIsNull**
   - Проверяет обработку отсутствия категории (null).

**Особенности реализации:**
- Использован `NSubstitute` для мокирования `IProductService` и `ICategoryService`.
- Использован `DefaultHttpContext` для мокирования `ControllerContext` (необходимо для работы расширения `IsAjaxRequest`).

---

## Задание 3: Тестирование сервиса получения продуктов

### Описание
Разработаны модульные тесты для обработчика `GetListOfProductsHandler` (аналог `ProductService` в архитектуре CQRS), отвечающего за получение списка продуктов из базы данных.

### Реализация

**Файл:** `CPPR.Tests/GetListOfProductsHandlerTests.cs`

**Используемые технологии:**
- **SQLite In-Memory**: Создается база данных в памяти для каждого теста, что обеспечивает изоляцию и быстродействие.
- **EF Core**: Используется реальный `AppDbContext` с подключением к SQLite.

**Проверяемые сценарии:**
1. **Handle_ReturnsFirstPageOfThreeItems_WhenNoCategory**
   - Проверяет пагинацию по умолчанию (1-я страница, 3 элемента).
   - Проверяет расчет общего количества страниц.

2. **Handle_ReturnsCorrectPage_WhenPageIsSpecified**
   - Проверяет получение конкретной страницы (например, 2-й).
   - Проверяет корректность возвращаемых данных для этой страницы.

3. **Handle_FiltersByCategory**
   - Проверяет фильтрацию блюд по категории.
   - Убеждается, что возвращаются только блюда указанной категории.

4. **Handle_ReturnsError_WhenPageNumberTooHigh**
   - Проверяет обработку запроса несуществующей страницы.
   - Ожидается возврат ошибки "No such page".

---

## Результаты

✅ Создан и настроен проект тестирования `CPPR.Tests`
✅ Реализованы тесты для контроллера `ProductController` (4 теста)
✅ Реализованы тесты для бизнес-логики `GetListOfProductsHandler` (4 теста)
✅ Все 8 тестов успешно проходят проверку (`dotnet test`)

### Скриншот выполнения тестов
```
Passed!  - Failed:     0, Passed:     8, Skipped:     0, Total:     8, Duration: 1 s - CPPR.Tests.dll (net8.0)
```
