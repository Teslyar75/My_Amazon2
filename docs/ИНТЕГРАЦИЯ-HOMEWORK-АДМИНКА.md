# Интеграция homework → диплом Perry (Figma)

Источник логики: [HomeWork_25.10.2025](https://github.com/Teslyar75/HomeWork_25.10.2025.git)  
Визуал: макет Figma «Дипломная работа» (бренд Perry / Amazon-like).

Клон для справки лежит в `_ref/HomeWork_25.10.2025` (в `.gitignore`).  
В Perry перенесено **по паттерну** (Domain / Infrastructure / Web / Api), БД — **SQL Server LocalDB**, не SQLite homework.

---

## 1. Карта Figma → код

| Экран Figma | В Perry |
|-------------|------------|
| Desktop - Main | `/` — hero, категории, deals, best sellers, **Recently viewed** |
| Product List Page | `/Products` — сайдбар **category / brand / price / rating**, sort, пагинация |
| Product Page | `/Products/Details/{id\|slug}` — галерея, Add to cart, Create review, related, viewed |
| Cart V2 | `/Cart` — guest / auth, checkout, viewed |
| Auth (Login / Sign up) | `/Account/Login`, `/Register` |
| Profile | `/Account/Profile` — edit + soft-delete + recent orders |
| Order history / details | `/Orders`, `/Orders/Details/{id}` + **Buy again** |
| Admin Category | `/Admin` — create + deactivate/activate |
| Admin Product | `/Admin` create, `/Admin/Edit/{id}`, archive/restore |
| Admin Order | `/Admin/Orders` — смена статуса |
| Admin User | `/Admin/Users` |

---

## 2. Что взято из homework

| Модуль homework | Файл(ы) в HW (ориентир) | В Perry |
|-----------------|-------------------------|------------|
| User / UserAccess / UserRole + seed Admin | `Data/Entities/*`, seed | Domain + миграция `AddAdminUsers` |
| Session-auth | `Middleware/AuthSessionMiddleware.cs` | `Web/Middleware/AuthSessionMiddleware.cs` |
| PbKdf1 | `Services/Kdf/*` | `Infrastructure/Auth/PbKdf1Service.cs` |
| DiskStorage | `Services/Storage/*` | `Infrastructure/Storage/DiskStorageService.cs` |
| Shop/Admin dashboard | `Views/Shop/Admin.cshtml` | `Pages/Admin/Index` |
| Shop/AdminEdit | `Views/Shop/AdminEdit.cshtml`, API AdminEdit | `Pages/Admin/Edit` |
| Soft-delete product / group | `ProductService.SoftDelete*`, `DeletedAt` | `ProductStatus.Archived` + Restore; `Category.IsActive` |
| Cart + guest session | `CartService`, Controllers | `ICartService` + cookie `ds_cart_sid` + merge на Login |
| Orders + RepeatOrder | `OrderService`, `Orders/*` | `IOrderService` + Buy again |
| ViewedProducts | `ViewedProductsService`, `_ViewedProducts` | `IViewedProductsService` + `_ViewedProducts.cshtml` |
| Product.Slug / Group.Slug | `Product.Slug`, `ProductGroup.Slug` | `Product.Slug`, `Category.Slug` |
| Related 3+3 | `GetRelatedProductsAsync` | `IProductService.GetRelatedAsync` |
| User update / soft-delete | `UserService`, `User/Profile` | `IUserService` + Profile handlers |
| Api Cart / Product / Group | `Controllers/Api/*` | `CartController`, Products/Categories POST+DELETE |
| Catalog filters | (слабее в HW; усилено под Figma) | сайдбар Web + query API |

### SKIP (не переносили)

- IoC / Razor / Privacy demos (`HomeController`)
- Учебный RateLimiting middleware
- SQLite `app.db`, монолит `DataAccessor`
- jQuery/Bootstrap vendor, 1000+ строк inline JS Admin
- Папки `ОТЧЕТ_*.md`, tar.gz
- Wishlist — **в homework его нет**

---

## 3. Соответствие soft-delete

| Homework | Perry |
|----------|----------|
| `Product.DeletedAt` | `Product.Status = Archived` (+ Restore → Active/OutOfStock) |
| `ProductGroup.DeletedAt` | `Category.IsActive = false` (+ Activate) |
| `User.DeletedAt` | `User.DeletedAtUtc` (Login не пускает удалённых) |

На витрине Archived и неактивные категории не показываются.

---

## 4. Auth и корзина

1. Покупатель: форма `/Account/Login` → Session key `SignIn` → Claims (`Id`, Role…).  
2. Админ: тот же механизм; роль `Admin`; страница `/Admin/Login`.  
3. Гость: cookie **`ds_cart_sid`**; корзина по `SessionId`.  
4. После логина: `MergeGuestToUserAsync` переносит позиции гостя к `UserId`.  
5. Выход: `?logout=1` (с `/Admin*` → `/Admin/Login`, иначе → `/`).

Пароль админа seed: **`Admin` / `Admin`** (PbKdf1, как в homework).

---

## 5. Как проверить интеграцию

```bash
dotnet run --project src/Perry.Web
dotnet run --project src/Perry.Api
```

| # | Сценарий | Ожидание |
|---|----------|----------|
| 1 | Каталог + фильтры | Список сужается |
| 2 | PDP по slug | URL вида `/Products/Details/3941r2-…` |
| 3 | Add to cart (гость) | Бейдж, `/Cart` |
| 4 | 2–3 просмотра | Recently viewed на главной |
| 5 | Register → Login | Корзина смержилась |
| 6 | Checkout → Buy again | Заказ + позиции снова в корзине |
| 7 | Profile Save / Delete | Имя обновилось / soft-delete |
| 8 | Admin Archive/Restore | Товар скрыт / снова на витрине |
| 9 | Admin Users / Orders | Списки заполнены |
| 10 | Swagger cart/products | 200 / Created |

БД: `(localdb)\mssqllocaldb` → **`Perry`**.

---

## 6. Где смотреть код

| Тема | Путь |
|------|------|
| DI сервисов | `src/Perry.Infrastructure/DependencyInjection.cs` |
| Cart / Order / Viewed / Product / User | `Infrastructure/Services/*.cs` |
| Session auth | `Web/Middleware/AuthSessionMiddleware.cs` |
| Витрина | `Web/Pages/Products`, `Cart`, `Account`, `Orders` |
| Админка | `Web/Pages/Admin/*` |
| API | `Api/Controllers/*` |
| Seed + slug backfill | `Infrastructure/Persistence/DbSeeder.cs` |

Полная карта API и страниц: [ПРОДЕЛАННАЯ-РАБОТА.md](./ПРОДЕЛАННАЯ-РАБОТА.md).
