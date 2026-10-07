# Perry Mobile (Expo)

Клиент покупателя на **Expo / React Native** под тот же Auth + Product API, что и web-витрина.  
Макеты: Figma iPhone frames (`cF0bKFsmenH6rrshGV0yO7`).  
План: `docs/МОБИЛЬНОЕ-ПРИЛОЖЕНИЕ-REACT.md` · решение: `docs/РЕШЕНИЕ-MOBILE-С-КОДОМ.md`.  
**Отчёт 01.10:** `docs/ОТЧЁТ-2026-10-01.md`.  
**Отчёт 07.10 (LAN / wishlist / #M09):** `docs/журнал/2026-10-07.md` · готовность проекта **~96%**.

**Вне скоупа:** админка. Internal Auth (#97) — Done на Product API.

## Быстрый запуск

Двойной клик по ярлыку **Perry Mobile** (корень репо / рабочий стол) → http://localhost:8081  
Или: `..\start-mobile.cmd` · установка ярлыков: `..\Install-Perry-Shortcuts.ps1`.  
Нужен **Perry.Api** на `:5272`.

## Запуск (вручную)

```bash
cd mobile
cp .env.example .env
# поправьте EXPO_PUBLIC_PRODUCT_URL под среду
npm start
# затем: a (Android) / i (iOS) / w (web)
```

| Среда | Product URL |
|-------|-------------|
| Expo Web (`:8081`) | Metro proxy `/api` → ПК |
| Expo Go (телефон) | авто: LAN IP ПК из Metro (`192.168.x.x:5272`) |
| Android emulator | fallback `http://10.0.2.2:5272` |

Auth по умолчанию — Azure Auth Service (как web).

**Expo Go:** телефон и ПК в одной Wi‑Fi; API слушает `0.0.0.0:5272` (не только localhost).  
Красная плашка «Product unreachable» = телефон бил в `localhost` на себе или API не слушал LAN.

## Что уже есть (MVP каркас)

| Экран | Figma / роль |
|-------|----------------|
| Home | iPhone Main — категории, trending, CTA |
| Catalog | Product List 2 колонки |
| PDP | галерея swipe, buy-box, wishlist |
| Cart / Checkout / Orders | guest session + merge + checkout |
| Login / Register / Forgot | Sign up & Log in - Mobile |
| Menu guest/customer | iPhone Menu |
| Account / Wishlist / Reviews | P1 stubs wired to API |

API-слой — порт с `perry-front` `src/api/*` + SecureStore + absolute media URLs.

## Структура

```text
mobile/
  App.tsx
  src/api/          # client, token, media, types, endpoints
  src/auth/         # AuthContext
  src/cart/         # CartContext
  src/components/   # Header, ProductCard, MenuDrawer, PrimaryButton
  src/navigation/   # tabs + stack
  src/screens/      # Home, Products, Product, Cart, Checkout, Auth, Account…
  src/theme/        # Perry colors
```

## Smoke

1. `GET {PRODUCT}/api/health`  
2. Home показывает товары  
3. Login → JWT в SecureStore  
4. PDP → To cart → Checkout → My orders  
5. PDP → сердце (wishlist) при логине → `POST /api/wishlist` 200  

## EAS (#M09)

Статус **07.10:** подготовка готова (`eas.json`, splash, deep link, LAN origin). Cloud build ждёт `eas login` (аккаунт Expo).

```bash
cd mobile
npx eas-cli@latest login
npx eas-cli@latest init          # один раз — привязка projectId на expo.dev
npx eas-cli@latest build -p android --profile preview
```

Профили в `eas.json`: `development` / `preview` (APK) / `production` (AAB).  
Deep link: `perry://product/{id}`. Splash/иконки — в `app.json` + `assets/`.

Для preview на эмуляторе задайте `EXPO_PUBLIC_PRODUCT_URL` в EAS secrets или пересоберите с `.env` (на устройстве — LAN IP ПК, не `localhost`).

## Примечание по навигации

Корневой stack + tabs; Menu открывает экраны Login/Orders/Wishlist и т.д.  
Пиксель-перфект Figma можно добить итерациями — сейчас приоритет: данные с API + layout 390px.
