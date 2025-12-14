# Ответы на вопросы для самопроверки (Лабораторная работа №7)

### 1. Что такое tag-helper?
Tag Helper (вспомогательный класс тега) — это класс C#, который позволяет серверному коду участвовать в создании и рендеринге HTML-элементов в Razor-файлах. Они делают код Razor более похожим на стандартный HTML.

### 2. Какие tag-helpers тэга `<form>` вы знаете?
- `asp-controller`: Имя контроллера для отправки формы.
- `asp-action`: Имя действия (метода).
- `asp-route-{value}`: Параметры маршрута.
- `asp-antiforgery`: Включение/отключение токена защиты от подделки.
- `method`: HTTP метод (post/get).

### 3. Какие tag-helpers тэга `<a>` вы знаете?
- `asp-controller`
- `asp-action`
- `asp-area`
- `asp-page` (для Razor Pages)
- `asp-route-{value}`
- `asp-fragment`
- `asp-protocol`

### 4. Как зарегистрировать использование Tag-helper в проекте?
В файле `_ViewImports.cshtml` добавить директиву:
```cshtml
@addTagHelper *, ИмяСборкиПроекта
```
Или для конкретного хелпера:
```cshtml
@addTagHelper Полное.Имя.Класса, ИмяСборки
```

### 5. Как определить, что запрос был выполнен по технологии Ajax?
Проверить наличие заголовка `X-Requested-With` со значением `XMLHttpRequest`.
```csharp
if (Request.Headers["X-Requested-With"] == "XMLHttpRequest") { ... }
```

### 6. Может ли MVC контроллер вернуть частичное представление?
Да, с помощью метода `PartialView()`.

### 7. Может ли MVC контроллер вернуть ответ без тела сообщения? Если да, приведите пример.
Да.
- `return StatusCode(200);`
- `return NoContent();` (код 204)
- `return Ok();` (если без параметров, тело пустое, но статус 200)

### 8. Приведите пример, когда контроллер возвращает ответ, не являющийся представлением.
- `return Json(data);` (JSON данные)
- `return File(...);` (Файл)
- `return RedirectToAction(...);` (Перенаправление)
- `return Content("Hello");` (Простой текст)
- `return BadRequest();` (Ошибка 400)

### 9. Можно ли с помощью tag-helper поменять тэг элемента?
Да, свойство `TagName` объекта `TagHelperOutput` позволяет изменить имя тега, который будет сгенерирован.
```csharp
output.TagName = "div";
```

### 10. Как в tag-helper получить Url конечной точки?
Нужно внедрить сервис `LinkGenerator` (или `IUrlHelperFactory`) в конструктор Tag Helper'а и использовать его методы (например, `GetPathByAction`, `GetPathByPage`).
