## Automatic Zoom

**Кафедра информатики. Гламаздин И.И.**

---

### `"applicationUrl": "https://localhost:7001;http://localhost:5001",`

### `"environmentVariables": { "ASPNETCORE_ENVIRONMENT": "Development" }`

---

## 3.4. Создание контроллера и представления

a. Создайте контроллер с именем **Home**
b. Создайте представление для метода **Index** созданного контроллера, без использования страницы шаблона, выводящее статический текст *"Hello World!"*
(предварительно создайте папку **Home** в папке **Views**)
c. Запустите проект и проверьте результат

---

## 3.5. Разметка представления Index

Подключите таблицы стилей:

* `lib/bootstrap/dist/css/bootstrap.min.css`
* `lib/bootstrap-icons/font/bootstrap-icons.min.css`
* `css/site.css`

Подключите скрипты:

* `lib/jquery/dist/jquery.min.js`
* `lib/bootstrap/dist/js/bootstrap.bundle.min.js`

Пример разметки:

```cshtml
@{
    Layout = null;
}
<!DOCTYPE html>
<html>
<head>
    <meta name="viewport" content="width=device-width" />
    <title>Index</title>
    <link rel="stylesheet" href="~/lib/bootstrap/dist/css/bootstrap.min.css" />
    <link rel="stylesheet" href="~/css/site.css" asp-append-version="true" />
    <link rel="stylesheet" href="~/lib/font-awesome/css/all.min.css" />
    <link rel="stylesheet" href="~/WebLabs_BSUIR_V02.styles.css" asp-append-version="true" />
</head>
<body>
    Hello World!
    <script src="~/lib/jquery/dist/jquery.min.js"></script>
    <script src="~/lib/bootstrap/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/site.js" asp-append-version="true"></script>
</body>
</html>
```

В файле **site.css** удалите все содержимое и добавьте:

```css
header, footer {
    background-color: rgba(var(--bs-dark-rgb));
}

.nav-color {
    color: rgba(255, 255, 255, 0.5);
}
```

---

## 3.6. Разметка структуры страницы

Структура:

```
<header>
  <div class="container">
    <!-- Панель навигации -->
    <span class="nav-color">Заголовок страницы</span>
  </div>
</header>

<main class="container">
  Содержимое страницы
</main>

<footer>
  <div class="container">
    <span class="nav-color">Подвал страницы</span>
  </div>
</footer>
```

---

## 3.7. Оформление заголовка страницы

Заголовок содержит:

* меню сайта
* информацию пользователя

Пример меню Bootstrap:

```cshtml
<nav class="navbar bg-dark navbar-expand-md border-bottom border-body" data-bs-theme="dark">
    <a class="navbar-brand" href="#">LabsV03</a>

    <div class="navbar-nav">
        <a class="nav-item nav-link active" href="#">Лб 1</a>
        <a class="nav-item nav-link" href="#">Каталог</a>
        <a class="nav-item nav-link" href="#">Администрирование</a>
    </div>

    <a href="#" class="navbar-text ms-auto">00,0 руб <i class="bi bi-cart"></i> (0)</a>

    <div class="dropdown ms-4 nav-color">
        <button class="btn btn-secondary dropdown-toggle" type="button"
                data-bs-toggle="dropdown">
            User@gmail.com
            <img src="images/default-profile-picture.png" width="30" class="rounded nav-color" />
        </button>

        <ul class="dropdown-menu">
            <li>
                <span class="dropdown-item-text">
                    <img src="images/default-profile-picture.png" width="50" class="rounded" />
                    user@gmail.com
                </span>
            </li>

            <li><hr class="dropdown-divider" /></li>

            <li>
                <form id="logoutForm" method="post">
                    <button id="logout" type="submit" class="nav-link btn btn-link text-dark">
                        Logout
                    </button>
                </form>
            </li>
        </ul>
    </div>
</nav>
```

---

## 3.8. Разметка подвала страницы

```cshtml
<footer>
    <div class="container">
        <div class="navbar bg-dark navbar-expand-md border-bottom border-body" data-bs-theme="dark">
            <div class="navbar-nav ms-auto">
                <a class="nav-item nav-link" href="http://www.facebook.com">
                    <i class="bi bi-facebook" style="font-size: 2rem"></i>
                </a>
                <a class="nav-item nav-link" href="http://www.vk.com">
                    <i class="bi bi-twitter" style="font-size: 2rem"></i>
                </a>
                <a class="nav-item nav-link" href="http://www.twitter.com">
                    <i class="bi bi-telegram" style="font-size: 2rem"></i>
                </a>
            </div>
        </div>
    </div>
</footer>
```

---

## 3.9. Разметка содержимого страницы

Используйте:

* `<ol>` с нумерацией заглавными буквами
* все элементы формы внутри `<form>`
* метод отправки `get`
* bootstrap — для верстки

---

## 3.10. Использование Developer Tools

1. Измените метод формы на POST
2. Откройте F12 → вкладка Network
3. Найдите Form Data
4. Убедитесь, что данные передаются

---

## 3.11. Окончательный вид страницы

*(пример приводился в оригинальном документе)*

---

## 3.12. Создание страницы-макета

1. Удалите или переименуйте старый `_Layout.cshtml`
2. Создайте новый Layout
3. В `<head>` подключите стили
4. В `<body>` вставьте разметку из Index, заменив содержимое `<main>` на `@RenderBody()`
5. Добавьте `@RenderSection("Scripts", required: false)`
6. Убедитесь, что `_ViewStart.cshtml` использует новый Layout

---

## 3.13. Использование страницы-макета

1. В Index оставить только содержимое `<main>`
2. Добавить:

```cshtml
@{
    ViewBag.Title = "Index";
}
```

3. Проверить, что внешний вид не изменился

---

# 4. Вопросы для самопроверки

1. В какой папке проекта хранятся файлы представлений контроллера?
2. Где в проекте должен находиться статический контент?
3. Где в проекте создается конвейер обработки запросов?
4. Где в проекте размещаются файлы классов контроллеров?
5. Для чего используется файл `_ViewStart.cshtml`?
6. Для чего используется файл `_ViewImports.cshtml`?
7. Для чего используется файл макета (Layout)?
8. Как в Layout указать место, куда будет помещена разметка представления?
9. Что такое «Section» на макете?
10. Как использовать секцию в представлении?
11. Как указать, что представление использует Layout?
12. Как указать, что представление не использует Layout?
13. Что означает утверждение: «HTTP не поддерживает сохранение состояний»?
14. Как в form указать адрес отправки данных?
15. В каком виде данные передаются на сервер?
16. Разница между POST и GET?

