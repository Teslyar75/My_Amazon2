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
dotnet run --project src/DuSoleil.Web
```

- Витрина: http://localhost:5122/  
- Админ: `/Admin/Login` — `Admin` / `Admin`  
- API: `dotnet run --project src/DuSoleil.Api` → `/swagger`

БД: `(localdb)\mssqllocaldb` → `DuSoleil`.
