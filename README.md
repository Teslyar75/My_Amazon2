# DuSoleil

Дипломный маркетплейс (зона товаров) на **ASP.NET Core 8** — витрина + админка + REST API по макету Figma, с паттернами из [HomeWork_25.10.2025](https://github.com/Teslyar75/HomeWork_25.10.2025.git).

## Документация

Вся подробная документация в папке **[docs/](./docs/)**:

| Файл | О чём |
|------|--------|
| [docs/README.md](./docs/README.md) | Оглавление и быстрый старт |
| [docs/ПРОДЕЛАННАЯ-РАБОТА.md](./docs/ПРОДЕЛАННАЯ-РАБОТА.md) | Архитектура, сущности, API, витрина, чеклист |
| [docs/КАК-ВЫПОЛНЯТЬ-ЗАДАНИЕ.md](./docs/КАК-ВЫПОЛНЯТЬ-ЗАДАНИЕ.md) | Запуск, smoke-тесты, что доделать по желанию |
| [docs/ИНТЕГРАЦИЯ-HOMEWORK-АДМИНКА.md](./docs/ИНТЕГРАЦИЯ-HOMEWORK-АДМИНКА.md) | Что перенесено из homework |

## Запуск

```bash
dotnet run --project src/DuSoleil.Web --launch-profile http
```

Открывай **http://localhost:5122/** (не https — иначе браузер может показать «нет доступа»).

- Админ: `/Admin/Login` — `Admin` / `Admin`  
- API: `dotnet run --project src/DuSoleil.Api` → `/swagger`

БД: `(localdb)\mssqllocaldb` → `DuSoleil`.

## Скриншоты

Подробные описания: **[docs/screenshots/README.md](./docs/screenshots/README.md)**

| Экран | Превью |
|-------|--------|
| Главная | ![Home](./docs/screenshots/01-home.png) |
| Каталог + фильтры | ![Catalog](./docs/screenshots/02-catalog.png) |
| Sign in | ![Login](./docs/screenshots/03-login.png) |
| Admin Dashboard | ![Admin](./docs/screenshots/04-admin-dashboard.png) |
| Admin категории/товары | ![Admin tables](./docs/screenshots/05-admin-catalog.png) |
| Admin Users | ![Users](./docs/screenshots/06-admin-users.png) |
| Product Page + Add to cart | ![PDP](./docs/screenshots/07-product-details.png) |
| Cart | ![Cart](./docs/screenshots/08-cart-update.png) |
| Profile | ![Profile](./docs/screenshots/09-profile.png) |
| Related / Best sellers | ![Related](./docs/screenshots/10-related-products.png) |
| Cart (несколько позиций) | ![Cart full](./docs/screenshots/11-cart-full.png) |
