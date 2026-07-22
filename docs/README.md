# Документация DuSoleil

Маркетплейс **Du Soleil** (диплом, зона товаров) — ASP.NET Core 8, макет Figma, паттерны из [HomeWork_25.10.2025](https://github.com/Teslyar75/HomeWork_25.10.2025.git).

| Файл | Содержание |
|------|------------|
| [ПРОДЕЛАННАЯ-РАБОТА.md](./ПРОДЕЛАННАЯ-РАБОТА.md) | Архитектура, сущности, API, витрина, админка, сервисы, миграции |
| [КАК-ВЫПОЛНЯТЬ-ЗАДАНИЕ.md](./КАК-ВЫПОЛНЯТЬ-ЗАДАНИЕ.md) | Как запустить, сценарии проверки, что осталось по желанию |
| [ИНТЕГРАЦИЯ-HOMEWORK-АДМИНКА.md](./ИНТЕГРАЦИЯ-HOMEWORK-АДМИНКА.md) | Что перенесено из homework → DuSoleil |

---

## Быстрый старт

```bash
cd d:\Amazon2
dotnet run --project src/DuSoleil.Web
```

| Что | URL / данные |
|-----|----------------|
| Витрина | http://localhost:5122/ (порт смотри в консоли) |
| Каталог | `/Products` |
| Админка | `/Admin/Login` — **`Admin` / `Admin`** |
| Покупатель | `/Account/Register` → Login |
| API + Swagger | `dotnet run --project src/DuSoleil.Api` → `/swagger` |

БД: `(localdb)\mssqllocaldb` → **`DuSoleil`**.
