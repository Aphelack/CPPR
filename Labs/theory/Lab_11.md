# Лабораторная работа №11
## Введение в Blazor

### Выполненные задачи

---

## Задание 1: Создание проекта Blazor

### Описание
В решение добавлен новый проект `CPPR.Blazor.SSR` типа **Blazor Web App**.
Выбран режим рендеринга **Interactive Server** (Server-Side Rendering с интерактивностью через WebSocket).

### Структура проекта
- **Program.cs**: Настройка сервисов Blazor (`AddRazorComponents`, `AddInteractiveServerComponents`) и конвейера (`MapRazorComponents`, `AddInteractiveServerRenderMode`).
- **Components/App.razor**: Корневой компонент.
- **Components/Routes.razor**: Маршрутизация.
- **Components/Pages/**: Страницы приложения (Home, Counter, Weather).
- **Components/Layout/**: Макеты (MainLayout, NavMenu).

---

## Задание 2: Модификация компонента Counter

### Описание
Компонент `Counter.razor` был доработан для установки значения счетчика вручную с валидацией.

### Реализация

**Форма ввода:**
Использован компонент `EditForm` с привязкой к модели `CounterModel`.
```razor
<EditForm Model="@Model" OnValidSubmit="@SetCount">
    <DataAnnotationsValidator />
    <ValidationSummary />
    <InputNumber @bind-Value="Model.CountValue" ... />
    <button type="submit">Set</button>
</EditForm>
```

**Модель данных:**
Создан вложенный класс `CounterModel` с атрибутами валидации.
```csharp
public class CounterModel
{
    [Range(1, 10, ErrorMessage = "Value must be between 1 and 10")]
    public int CountValue { get; set; }
}
```

**Логика:**
Метод `SetCount` обновляет текущее значение счетчика только при успешной валидации формы.

---

## Задание 3: Параметры строки запроса

### Описание
Реализована возможность инициализации счетчика через параметр строки запроса `initialCount`.

### Реализация

Использован атрибут `[SupplyParameterFromQuery]`.

```csharp
[SupplyParameterFromQuery]
public int? InitialCount { get; set; }

protected override void OnInitialized()
{
    if (InitialCount.HasValue)
    {
        currentCount = InitialCount.Value;
    }
}
```

### Проверка
При переходе по адресу `/counter?initialCount=5` счетчик инициализируется значением 5.
