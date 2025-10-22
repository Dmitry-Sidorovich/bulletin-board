# 🎯 Bulletin Board (Электронная доска объявлений)

Веб-приложение для размещения объявлений с поддержкой категорий, поиска и загрузки файлов.

## 📋 Содержание

- [Технологии](#технологии)
- [Docker и Docker Compose](#-docker-и-docker-compose)
- [Архитектура](#-архитектура)
- [Быстрый старт](#-быстрый-старт)
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

## 🐳 Docker и Docker Compose

Проект полностью контейнеризирован для удобного развертывания.

### Архитектура Docker Compose

Стек состоит из **3 сервисов**, которые связаны в единую сеть:

```
┌─────────────────────────────────────────────────────────────┐
│              bulletinboard-network (bridge)                  │
├─────────────────┬──────────────────┬────────────────────────┤
│   PostgreSQL    │   Redis (Cache)  │   ASP.NET Core API    │
│   Port: 5432    │   Port: 6379     │   Port: 8080          │
│   Persistent    │   Persistent     │   Multi-stage build   │
│   Data Volume   │   Data Volume    │   Health checks       │
└─────────────────┴──────────────────┴────────────────────────┘
```

### Компоненты

#### 1. **PostgreSQL 17**
- Основная база данных приложения
- Персистентный том: `postgres_data`
- Health check каждые 5 секунд
- Переменные окружения: POSTGRES_USER, POSTGRES_PASSWORD, POSTGRES_DB

#### 2. **Redis 7** (Кэш)
- Распределенный кэш для гибридной системы кэширования
- Персистентный том: `redis_data`
- Health check через `redis-cli ping`
- Оптимизирует производительность запросов

#### 3. **ASP.NET Core API**
- Основное приложение на .NET 8
- Построено из `Dockerfile` (multi-stage build)
- Здоровье проверяется через HTTP `/health` эндпоинт
- Зависит от PostgreSQL и Redis (условие: service_healthy)
- Volumes для uploads и logs

### Dockerfile: Multi-stage Build

```dockerfile
# Stage 1: Build
- Использует SDK образ (329MB)
- Восстанавливает зависимости
- Компилирует в Release конфигурации
- Результат: /app/publish

# Stage 2: Runtime
- Использует aspnet образ (94MB вместо 329MB)
- Копирует только готовое приложение
- Создает директорию для uploads
- Итоговый размер образа: ~500MB
```

**Преимущества multi-stage build:**
- ✅ Меньший размер финального образа (исключены SDK инструменты)
- ✅ Быстрее развертывается и скачивается
- ✅ Безопаснее (нет исходного кода в образе)
- ✅ Пригоден для production

### Health Checks

Все сервисы имеют настроенные health checks:

```yaml
PostgreSQL:
  - Команда: pg_isready -U postgres -d bulletin_board
  - Интервал: 5s
  - Timeout: 3s
  - Retries: 10

Redis:
  - Команда: redis-cli ping
  - Интервал: 5s
  - Timeout: 3s
  - Retries: 10

API:
  - Endpoint: http://localhost:8080/health
  - Интервал: 30s
  - Timeout: 10s
  - Start period: 40s
  - Retries: 3
```

API стартует только после того, как PostgreSQL и Redis прошли health check.

### Структура томов

```
postgres_data/      - Данные PostgreSQL (БД, индексы)
redis_data/         - Данные Redis (кэш)
./uploads/          - Загруженные файлы приложения
./logs/             - Логи приложения
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

⚠️ **Важно для production:** Измените `Jwt__Secret` и пароли БД на безопасные значения!

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

**Основные паттерны:**
- Repository Pattern
- CQRS (Read/Write репозитории)
- Dependency Injection
- Unit of Work

---

## ⚡ Быстрый старт

### Требования

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Docker Desktop](https://www.docker.com/products/docker-desktop) (опционально)
- [PostgreSQL 15+](https://www.postgresql.org/download/) (если без Docker)

### Вариант 1: Запуск с Docker Compose 🐳

**Требования:** Docker Desktop

```bash
# 1. Клонировать репозиторий
git clone https://github.com/Dmitry-Sidorovich/bulletin-board.git
cd bulletin-board

# 2. Запустить весь стек через Docker Compose
docker-compose up -d

# 3. Дождитесь, пока все сервисы загрузятся (проверьте статус здоровья)
docker-compose ps

# 4. API доступен по адресу:
# API:       http://localhost:8080
# Swagger:   http://localhost:8080/swagger
```

**Что запускается:**
- **PostgreSQL 17** - база данных (порт 5432)
- **Redis 7** - кэш (порт 6379)
- **ASP.NET Core API** - приложение (порт 8080)

**Автоматическая инициализация БД:**
- БД создаётся автоматически при первом запуске API
- Все таблицы инициализируются через EF Core (`MigrateAsync()` + `EnsureCreatedAsync()`)
- Нет необходимости запускать отдельный миграционный скрипт

**Полезные команды:**
```bash
# Просмотр логов API (в том числе инициализацию БД)
docker-compose logs -f api

# Остановить все сервисы
docker-compose down

# Переустроить образ API (если были изменения)
docker-compose up -d --build

# Удалить также все данные (включая БД)
docker-compose down -v

# Проверить статус контейнеров
docker-compose ps

# Проверить таблицы в БД
docker-compose exec postgres psql -U postgres -d bulletin_board -c "\dt"
```

**Конфигурация при Docker Compose:**
- PostgreSQL подключение: `Host=postgres;Port=5432;Database=bulletin_board;Username=postgres;Password=password`
- Redis: `redis:6379`
- Окружение: `Development` (для включения Swagger и логирования)
- JWT Secret и другие переменные окружения установлены в `docker-compose.yml`

**Точки входа API:**
- Swagger UI: http://localhost:8080/swagger
- API Root: http://localhost:8080
- Реальные эндпоинты:
  - `GET /api/advertisements` - список объявлений
  - `GET /api/categories` - список категорий
  - `POST /api/auth/register` - регистрация
  - `POST /api/auth/login` - вход

### Вариант 2: Локальный запуск

**Требования:**
- .NET 8 SDK
- PostgreSQL 15+ (запущенный локально)
- Redis 7+ (опционально, если хотите использовать гибридный кэш)

```bash
# 1. Клонировать репозиторий
git clone https://github.com/Dmitry-Sidorovich/bulletin-board.git
cd bulletin-board

# 2. Создать БД PostgreSQL
createdb bulletin_board

# 3. Обновить строку подключения в appsettings.Development.json
# "ConnectionStrings": {
#   "MainDb": "Host=localhost;Port=5432;Database=bulletin_board;Username=postgres;Password=your_password",
#   "Redis": "localhost:6379"  # если используется Redis
# }

# 4. Применить миграции
cd src/BulletinBoard/Hosts/BulletinBoard.Hosts.DbMigrator
dotnet run

# 5. Запустить API
cd ../BulletinBoard.Hosts.Api
dotnet run

# 6. Открыть Swagger
# http://localhost:5000/swagger (или 8080 в зависимости от конфигурации)
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

Проект имеет **335 тестов** с 100% успешным прохождением, включая:
- **246 Unit тестов** - тестирование отдельных компонентов и бизнес-логики
- **32 Integration тестов** - тестирование взаимодействия между слоями
- **57 Infrastructure/Security тестов** - тестирование JWT, шифрования паролей и валидаторов

```bash
# Запустить все тесты
dotnet test

# Запустить только unit-тесты
dotnet test --filter "Category=Unit"

# Запустить только integration-тесты
dotnet test --filter "Category=Integration"

# С покрытием кода
dotnet test --collect:"XPlat Code Coverage"

# Просмотр результатов в реальном времени
dotnet test --verbosity normal
```

**Покрытие тестами:**
- Domain: 99%
- Application: 95%+
- Infrastructure: 100% (Security-критично)
- **Всего: 335 тестов**

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