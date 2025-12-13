# Лабораторная работа №10
## Журналирование

### Выполненные задачи

---

## Задание 1: Настройка Serilog

### Описание
В проект добавлена библиотека `Serilog` для ведения журналов событий. Настроено логирование в консоль и в файл.

### Реализация

**Установленные пакеты:**
- `Serilog.AspNetCore`

**Конфигурация (`appsettings.json`):**
Добавлена секция `Serilog`, определяющая уровни логирования и "синки" (приемники) для вывода логов:
- `Console`: Вывод в консоль.
- `File`: Вывод в файл с ротацией по дням (`Logs/log-.txt`).

**Регистрация в `Program.cs`:**
```csharp
builder.Host.UseSerilog((context, configuration) => 
    configuration.ReadFrom.Configuration(context.Configuration));
```

---

## Задание 2: Реализация Middleware для логирования

### Описание
Создан компонент middleware `LogMiddleware` для перехвата HTTP-запросов и логирования ответов с кодами ошибок (не 2xx).

### Реализация

**Файл:** `Project/Middleware/LogMiddleware.cs`

**Логика:**
Middleware проверяет статус код ответа (`context.Response.StatusCode`). Если код меньше 200 или больше либо равен 300, в журнал записывается информационное сообщение:
`"---> request {Path} returns {StatusCode}"`

**Регистрация в конвейере (`Program.cs`):**
Middleware добавлен в конвейер обработки запросов перед `UseSession`:
```csharp
app.UseMiddleware<LogMiddleware>();
```

---

## Задание 3: Проверка работоспособности

### Описание
Проведено тестирование системы логирования путем генерации запроса к несуществующему ресурсу.

### Результат
При запросе к `http://localhost:5197/some-random-page-for-404` сервер вернул статус 404.
В файле логов (`Project/Logs/log-YYYYMMDD.txt`) появилась запись:
`[INF] ---> request /some-random-page-for-404 returns 404`

Это подтверждает корректную работу middleware и конфигурации Serilog.
