# Лабораторная работа №12
## Разработка веб-приложений с использованием Blazor WebAssembly и SignalR

### Выполненные задачи

---

## Задание 1: Создание проекта Blazor WebAssembly

### Описание
В решение добавлен новый проект `CPPR.BlazorWasm` типа **Blazor WebAssembly Standalone App**.
Настроена аутентификация с использованием индивидуальных учетных записей.

### Структура проекта
- **Program.cs**: Настройка сервисов, `HttpClient` и OIDC аутентификации.
- **Services/DataService.cs**: Сервис для взаимодействия с API.
- **Components/**: Компоненты пользовательского интерфейса.
- **Pages/Catalog.razor**: Страница каталога товаров.
- **Pages/Game.razor**: Страница онлайн-игры.

---

## Задание 2: Интеграция с API и Сервис данных

### Описание
Разработан сервис `DataService`, реализующий интерфейс `IDataService`.
Сервис выполняет запросы к API для получения списка категорий и блюд.

### Реализация
```csharp
public async Task GetProductListAsync(int pageNo = 1)
{
    var url = "api/dish";
    if (SelectedCategory != null)
    {
        url += $"/{SelectedCategory.NormalizedName}";
    }
    url += $"?pageNo={pageNo}";

    var result = await _httpClient.GetFromJsonAsync<ResponseData<ListModel<Dish>>>(url);
    // ... обработка результата
}
```

---

## Задание 3: Компоненты пользовательского интерфейса

### Описание
Созданы переиспользуемые компоненты:
- **CategorySelector**: Выпадающий список для выбора категории.
- **DishesList**: Отображение списка блюд в виде карточек.
- **Pager**: Компонент пагинации.
- **DishDetails**: Модальное окно с детальной информацией.

---

## Задание 4: Онлайн-игра с использованием SignalR

### Описание
Реализована игра "Чак-э-лак" (Chuck-a-Luck).

### Серверная часть (API)
Создан хаб `GameHub`, обрабатывающий подключение к группам и логику игры (бросок костей).
```csharp
public async Task RollDice(string groupName, int bet, int number)
{
    // Логика броска 3-х костей и подсчет выигрыша
    await Clients.Group(groupName).SendAsync("GameResult", dice1, dice2, dice3, winnings);
}
```

### Клиентская часть (Blazor)
Создана страница `Game.razor`, использующая `HubConnection` для связи с сервером.
Реализовано:
- Подключение к игровой комнате.
- Ставки и выбор числа.
- Отображение результатов броска и выигрыша/проигрыша.
