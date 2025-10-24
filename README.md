# 🎯 Bulletin Board

Web API для электронной доски объявлений с поддержкой категорий, поиска и загрузки файлов.

## 📋 Содержание

- [Технологии](#технологии)
- [Архитектура](#-архитектура)
- [Быстрый старт](#-быстрый-старт)
- [Docker и Docker Compose](#-docker-и-docker-compose)

- [API Endpoints](#-api-endpoints)
- [Тестирование](#-тестирование)

---

## 🚀 Технологии

- **.NET 8** - платформа разработки
- **ASP.NET Core** - веб-фреймворк
- **Entity Framework Core** - ORM
- **PostgreSQL** - база данных
- **JWT** - аутентификация
- **FluentValidation** - валидация
- **AutoMapper** - маппинг объектов
- **Swagger** - документация API
- **xUnit** - юнит-тестирование
- **Docker** - контейнеризация

---

## 🏛️ Архитектура

Проект построен по принципам **Clean Architecture**:
```
BulletinBoard/
├── Domain/          # Доменные сущности, Value Objects
├── Application/     # Бизнес-логика, Use Cases
├── Infrastructure/  # Реализация репозиториев, EF Core
├── Contracts/       # DTO, контракты API
└── Hosts/
    ├── Api/        # REST API
    └── DbMigrator/ # Миграции БД
```


## 🐳 Docker и Docker Compose

Проект полностью контейнеризирован для удобного развертывания.

```

**Порядок запуска:**
1. PostgreSQL и Redis стартуют параллельно
2. DbMigrator ждёт когда PostgreSQL будет Healthy
3. DbMigrator применяет все миграции и завершает работу
4. API стартует после успешного завершения DbMigrator
```


### Переменные окружения (docker-compose.yml)

```yaml
# Database
ConnectionStrings__MainDb: Host=postgres;Port=5432;Database=bulletin_board;Username=postgres;Password=password

# Cache
ConnectionStrings__Redis: redis:6379
Caching__EnableRedis: "true"

# JWT
Jwt__Secret: your-secret-key-minimum-32-characters-long-12345
Jwt__Issuer: bulletinboard.api
Jwt__Audience: bulletinboard.client
Jwt__AccessTokenExpirationMinutes: 60
Jwt__RefreshTokenExpirationDays: 30
```

⚠️ **Важно для production:** Изменить `Jwt__Secret` и пароли БД на безопасные значения!

---

## ⚡ Быстрый старт

### Требования

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)

### Вариант 1: Запуск с Docker Compose 🐳 (Production-like)

**Требования:** Docker Desktop

```bash
# 1. Клонировать репозиторий
git clone https://github.com/Dmitry-Sidorovich/bulletin-board.git
cd bulletin-board

# 2. Запустить весь стек через Docker Compose
docker-compose up -d --build

# 3. Дождитесь, пока все сервисы загрузятся
docker-compose ps

# 4. Проверьте что DbMigrator завершил работу
docker-compose logs db-migrator --tail=10
# Должны увидеть: "✅ Migrations applied successfully!"

# 5. API доступен по адресу:
# API:       http://localhost:8080
# Swagger:   http://localhost:8080/swagger
```

**Что запускается:**
- **PostgreSQL 17** - база данных (порт 5432)
- **Redis 7** - кэш (порт 6379)
- **DbMigrator** - применяет миграции и завершает работу
- **ASP.NET Core API** - приложение (порт 8080)

**Процесс инициализации БД:**
1. PostgreSQL и Redis стартуют
2. DbMigrator ждёт когда PostgreSQL будет Healthy
3. DbMigrator применяет все миграции из папки `Migrations/`
4. DbMigrator завершает работу (exit код 0)
5. API стартует только после успешного завершения DbMigrator
6. БД полностью готова к использованию

**Точки входа API:**
- Swagger UI: http://localhost:8080/swagger
- API Root: http://localhost:8080
- Реальные эндпоинты:
  - `GET /api/advertisements` - список объявлений
  - `GET /api/categories` - список категорий
  - `POST /api/auth/register` - регистрация
  - `POST /api/auth/login` - вход

### Заполнение БД тестовыми данными

После того как все контейнеры запущены, можно заполнить БД демонстрационными данными (5 пользователей, 16 категорий, 12 объявлений).

**Использование Docker (рекомендуется):**

```bash
# Когда PostgreSQL запущен в Docker, используй cmd.exe (или WSL) вместо PowerShell:

# Вариант 1: Скопировать файл в контейнер и выполнить
docker cp seed-data.sql bulletinboard_postgres:/tmp/seed-data.sql
docker exec bulletinboard_postgres psql -U postgres -d bulletin_board -f /tmp/seed-data.sql

# Или Вариант 2: Напрямую через пайп (из cmd.exe)
type seed-data.sql | docker exec -i bulletinboard_postgres psql -U postgres -d bulletin_board
```

**Проверка что данные загрузились:**

```bash
docker exec bulletinboard_postgres psql -U postgres -d bulletin_board -c "SELECT 'Пользователи' as entity, COUNT(*) as count FROM users UNION ALL SELECT 'Категории', COUNT(*) FROM categories UNION ALL SELECT 'Объявления', COUNT(*) FROM advertisements;"
```

Должен вывести:
```
entity        | count
Пользователи  |     5
Категории     |    16
Объявления    |    12
```

**Тестовые учетные данные (пароль для всех: `Password123!`):**

| Email | Роль | Пароль |
|-------|------|--------|
| admin@bulletinboard.local | Admin | Password123! |
| ivan.ivanov@example.com | User | Password123! |
| maria.petrova@example.com | User | Password123! |
| alexey.sidorov@example.com | User | Password123! |
| elena.kuznetsova@example.com | User | Password123! |

**Содержание тестовых данных:**

- **Категории:** Электроника (Смартфоны, Ноутбуки, ТВ), Недвижимость (Квартиры, Дома), Транспорт (Авто, Мото, Велосипеды), Дом и сад (Мебель, Техника)
- **Объявления:** iPhone 15, MacBook Air, Samsung TV, квартира, дом, Toyota Camry, Yamaha MT-07, велосипед, диван, пылесос и др.
- **Статусы:** 9 активных объявлений, 1 черновик, 2 архивированных

### Вариант 2: Локальный запуск (Local Development)

#### PostgreSQL и Redis в Docker, API локально 

```bash
# 1. Клонировать репозиторий
git clone https://github.com/Dmitry-Sidorovich/bulletin-board.git
cd bulletin-board

# 2. Запустить ТОЛЬКО PostgreSQL и Redis в Docker (БЕЗ API и DbMigrator)
docker-compose up -d postgres redis

# 3. Проверить что сервисы ready
docker-compose ps
# postgres и redis должны быть Up и Healthy

# 4. Применить миграции (DbMigrator локально)
cd src/BulletinBoard/Hosts/BulletinBoard.Hosts.DbMigrator
dotnet run
# Должны увидеть: "✅ Migrations applied successfully!"

# 5. Запустить API локально
cd ../BulletinBoard.Hosts.Api
dotnet run
# API стартует на http://localhost:5000 (или 8080 в зависимости от конфигурации)

# 6. Открыть Swagger
# http://localhost:5000/swagger

# 7. Для остановки всех контейнеров
docker-compose down
```
---

## 📡 API Endpoints

### Аутентификация
```http
POST /api/auth/register  # Регистрация
POST /api/auth/login     # Вход
POST /api/auth/refresh   # Обновление токена
POST /api/auth/logout    # Выход
```

### Объявления
```http
GET    /api/advertisements              # Список (пагинация)
GET    /api/advertisements/{id}         # Получить по ID
GET    /api/advertisements/search       # Поиск с фильтрами
POST   /api/advertisements              # Создать (auth)
PUT    /api/advertisements/{id}         # Обновить (auth)
DELETE /api/advertisements/{id}         # Удалить (auth)
POST   /api/advertisements/{id}/files   # Прикрепить файл (auth)
```

### Категории
```http
GET  /api/categories         # Список корневых
GET  /api/categories/{id}    # Получить с подкатегориями
POST /api/categories         # Создать (admin)
```

### Файлы
```http
POST   /api/files           # Загрузить
GET    /api/files/{id}      # Скачать
DELETE /api/files/{id}      # Удалить (auth)
```

**Подробная документация:** [Swagger UI](http://localhost:5000/swagger)

---

## 🧪 Тестирование

### Локальное тестирование

```bash
# Запустить все тесты
dotnet test

# Запустить только unit-тесты
dotnet test tests/BulletinBoard.UnitTests/BulletinBoard.UnitTests.csproj

# Запустить только integration-тесты (требует PostgreSQL в Docker)
dotnet test tests/BulletinBoard.IntegrationTests/BulletinBoard.IntegrationTests.csproj

# С покрытием кода
dotnet test --collect:"XPlat Code Coverage"

# Просмотр результатов в реальном времени
dotnet test --verbosity normal
```

**Перед запуском Integration-тестов:**
```bash
# Убедитесь что PostgreSQL и Redis работают в Docker
docker-compose up -d postgres redis
docker-compose ps  # должны быть Up и Healthy
```

### Тестирование в Docker

**Unit-тесты в Docker (не требуют БД):**
```bash
# Собрать образ с тестами
docker build -t bulletinboard-tests -f Dockerfile .

# Запустить unit-тесты
docker run --rm bulletinboard-tests dotnet test tests/BulletinBoard.UnitTests/BulletinBoard.UnitTests.csproj
```

**Integration-тесты в Docker:**
```bash
# Требует PostgreSQL в Docker
docker-compose up -d postgres redis

# Запустить тесты
docker run --rm \
  --network bulletinboard-network \
  --env ConnectionStrings__MainDb="Host=postgres;Port=5432;Database=bulletin_board;Username=postgres;Password=password" \
  bulletinboard-tests \
  dotnet test tests/BulletinBoard.IntegrationTests/BulletinBoard.IntegrationTests.csproj
```

---

## 📦 Структура БД
```
advertisements (объявления)
├── id
├── title
├── description
├── price
├── category_id → categories
├── author_id → users
├── status
└── created_at

categories (категории)
├── id
├── name
└── parent_id → categories

users (пользователи)
├── id
├── display_name
├── email
├── password_hash
└── role

files (файлы)
├── id
├── file_name
├── file_path
└── content_type

advertisement_files (связь M2M)
├── advertisement_id → advertisements
├── file_id → files
└── order
```

---

## 👨‍💻 Автор

**Dmitry Sidorovich**
- GitHub: [@Dmitry-Sidorovich](https://github.com/Dmitry-Sidorovich)
- Email: dimasidorovich7@gmail.com

---

## 📄 Лицензия

MIT License

---

## 🙏 Благодарности

Проект разработан в рамках курса .NET Developer от СоларЛаб. 