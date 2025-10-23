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

Стек состоит из **4 сервисов**, которые связаны в единую сеть:

```
┌─────────────────────────────────────────────────────────────────────┐
│                  bulletinboard-network (bridge)                      │
├──────────────┬──────────────────┬─────────────────┬────────────────┤
│ PostgreSQL   │ Redis (Cache)    │ DbMigrator      │ ASP.NET API   │
│ Port: 5432   │ Port: 6379       │ (Job, exit)     │ Port: 8080    │
│ Persistent   │ Persistent       │ Миграции БД     │ Multi-stage   │
│ Data Volume  │ Data Volume      │                 │ Health checks │
└──────────────┴──────────────────┴─────────────────┴────────────────┘
                          ↓ (depends_on)
```

**Порядок запуска:**
1. PostgreSQL и Redis стартуют параллельно
2. DbMigrator ждёт когда PostgreSQL будет Healthy
3. DbMigrator применяет все миграции и завершает работу
4. API стартует после успешного завершения DbMigrator

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

#### 3. **DbMigrator** (новый!)
- Отвечает за применение всех миграций БД
- Работает как одноразовая задача (Job): запускается, применяет миграции, завершает работу
- Использует `MigrationDbContext` для отслеживания миграций
- **DOTNET_ENVIRONMENT=Production** (читает `appsettings.Production.json`)
- Гарантирует что БД готова перед стартом API
- Минимизирует время старта API

#### 4. **ASP.NET Core API**
- Основное приложение на .NET 8
- Построено из `Dockerfile` (multi-stage: 4 стейджа)
- Зависит от PostgreSQL, Redis и DbMigrator
- **Не применяет миграции** - это делает DbMigrator
- Volumes для uploads и logs

### Dockerfile: Multi-stage Build (4 стейджа)

```dockerfile
# STAGE 1: Build DbMigrator (SDK 8.0)
- Собирает только DbMigrator проект
- Восстанавливает зависимости отдельно
- Публикует в /app/migrator/publish

# STAGE 2: Build API (SDK 8.0)
- Собирает только API проект
- Восстанавливает зависимости отдельно
- Публикует в /app/api/publish
- Избегает конфликтов при публикации двух проектов

# STAGE 3: DbMigrator Runtime (aspnet 8.0)
- Использует aspnet образ (меньше SDK)
- Копирует только DbMigrator DLL
- Удаляет appsettings.Development.json (чтобы не было конфликтов)
- Копирует appsettings.Production.json (с Host=postgres)
- Копирует папку Migrations

# STAGE 4: API Runtime (aspnet 8.0)
- Использует aspnet образ
- Копирует только API DLL
- Создает директорию для uploads
- Итоговый размер образа: ~500MB
```

**Преимущества multi-stage build:**
- ✅ Меньший размер финального образа (исключены SDK инструменты)
- ✅ Быстрее развертывается и скачивается
- ✅ Безопаснее (нет исходного кода в образе)
- ✅ Пригоден для production
- ✅ Две отдельные сборки не конфликтуют с файлами друг друга

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

**Полезные команды:**
```bash
# Просмотр логов DbMigrator (проверить применение миграций)
docker-compose logs db-migrator --tail=20

# Просмотр логов API
docker-compose logs api --tail=30

# Следить за логами API в реальном времени
docker-compose logs -f api

# Остановить все сервисы
docker-compose down

# Пересобрать образы и запустить (если были изменения)
docker-compose up -d --build

# Удалить также все данные (БД будет пересоздана с нуля)
docker-compose down -v

# Проверить статус контейнеров
docker-compose ps

# Проверить таблицы в БД (убедиться что миграции применились)
docker-compose exec postgres psql -U postgres -d bulletin_board -c "\dt"

# Посчитать таблицы
docker-compose exec postgres psql -U postgres -d bulletin_board -c "SELECT COUNT(*) FROM information_schema.tables WHERE table_schema='public';"
```

**Конфигурация при Docker Compose:**
- PostgreSQL подключение: `Host=postgres;Port=5432;...` (используется имя сервиса)
- Redis: `redis:6379` (используется имя сервиса)
- DbMigrator: `DOTNET_ENVIRONMENT=Production` (читает `appsettings.Production.json` с Host=postgres)
- API: `ASPNETCORE_ENVIRONMENT=Development` (для Swagger)
- JWT Secret и другие переменные окружения установлены в `docker-compose.yml`

**Точки входа API:**
- Swagger UI: http://localhost:8080/swagger
- API Root: http://localhost:8080
- Реальные эндпоинты:
  - `GET /api/advertisements` - список объявлений
  - `GET /api/categories` - список категорий
  - `POST /api/auth/register` - регистрация
  - `POST /api/auth/login` - вход

### Вариант 2: Локальный запуск (Local Development)

**Требования:**
- .NET 8 SDK
- Docker Desktop (для PostgreSQL и Redis) ИЛИ PostgreSQL 15+ локально + Redis 7+

#### Способ 2A: PostgreSQL и Redis в Docker, API локально (рекомендуется)

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

**Преимущества этого способа:**
- ✅ БД всегда работает (легко пересоздать через `docker-compose down -v`)
- ✅ Меньше трафика (Docker не собирает образы)
- ✅ Можно быстро перезагружать код без пересборки контейнеров
- ✅ Debug и intellisense работают отлично

**Конфигурация при локальном запуске:**
- `appsettings.json` используется как базовый конфиг
- `appsettings.Development.json` переопределяет логирование для более подробных логов
- Оба файла читаются автоматически (DOTNET_ENVIRONMENT=Development по умолчанию)
- PostgreSQL подключение: `Host=localhost;Port=5432;Database=bulletin_board;Username=postgres;Password=password`
- Redis: `localhost:6379`

#### Способ 2B: Всё локально (PostgreSQL и Redis на хосте)

```bash
# 1. Убедитесь что PostgreSQL запущен (слушает на localhost:5432)
psql -U postgres -c "SELECT version();"

# 2. Создать БД
createdb bulletin_board

# 3. Клонировать репозиторий
git clone https://github.com/Dmitry-Sidorovich/bulletin-board.git
cd bulletin-board

# 4. Применить миграции
cd src/BulletinBoard/Hosts/BulletinBoard.Hosts.DbMigrator
dotnet run

# 5. Запустить API
cd ../BulletinBoard.Hosts.Api
dotnet run

# 6. Открыть Swagger
# http://localhost:5000/swagger
```

**Конфигурация:**
- Убедитесь что `appsettings.json` содержит правильные параметры подключения
- Redis опционален (если не нужен кэш, отключите в `appsettings.json`: `"Caching": { "EnableRedis": false }`)

**Чем отличаются способы:**
- **Способ 2A** (рекомендуется): PostgreSQL+Redis в Docker, API локально → легче разрабатывать
- **Способ 2B**: Всё на хосте → нужно устанавливать PostgreSQL и Redis локально

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

# Запустить тесты с фильтром по категории
dotnet test --filter "Category=Unit"
dotnet test --filter "Category=Integration"
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

**Покрытие тестами:**
- Domain: 99%
- Application: 95%+
- Infrastructure: 100% (Security-критично)
- **Всего: 335 тестов**

---

## 🔧 Troubleshooting

### Docker запуск

**Проблема: DbMigrator падает с ошибкой "Failed to connect to 127.0.0.1:5432"**

Причина: DbMigrator пытается подключиться к localhost вместо postgres (имя сервиса в Docker).

Решение:
```bash
# Убедитесь что в docker-compose.yml для db-migrator установлено:
environment:
  DOTNET_ENVIRONMENT: Production
  ConnectionStrings__MainDb: "Host=postgres;Port=5432;..."
```

**Проблема: API не может подключиться к БД "connection refused"**

Причина: API стартует до того как DbMigrator завершил работу.

Решение: Проверьте что в docker-compose.yml для api установлено:
```yaml
depends_on:
  db-migrator:
    condition: service_completed_successfully
```

**Проблема: "BulletinBoard.Hosts.DbMigrator assembly not found" при запуске API**

Причина: API пытается использовать MigrationsAssembly, который больше не нужен.

Решение: Убедитесь что в `ComponentRegistrar.cs` удалена строка:
```csharp
// ❌ Неправильно - удалите эту строку:
.UseMigrationsAssembly("BulletinBoard.Hosts.DbMigrator")

// ✅ Правильно:
.UseNpgsql(cs)
```

**Проблема: Хочу пересоздать БД с нуля**

Решение:
```bash
# Полностью удалить контейнеры и том с данными
docker-compose down -v

# Запустить заново (БД пересоздастся автоматически)
docker-compose up -d --build

# Проверить что миграции применились
docker-compose logs db-migrator --tail=5
```

### Локальный запуск

**Проблема: DbMigrator локально не применяет миграции**

Причина: Может быть неправильный конфиг подключения или БД не существует.

Решение:
```bash
# 1. Проверьте что PostgreSQL работает
docker-compose ps

# 2. Проверьте что БД существует
docker-compose exec postgres psql -U postgres -c "\l bulletin_board"

# 3. Проверьте appsettings.json DbMigrator
cat src/BulletinBoard/Hosts/BulletinBoard.Hosts.DbMigrator/appsettings.json
# Должно быть: "Host=localhost" (если PostgreSQL локально в Docker)

# 4. Запустите DbMigrator с verbose логами
cd src/BulletinBoard/Hosts/BulletinBoard.Hosts.DbMigrator
DOTNET_ENVIRONMENT=Development dotnet run --verbosity Debug
```

**Проблема: "Database bulletin_board does not exist" при запуске DbMigrator**

Решение:
```bash
# Создать БД через Docker
docker-compose exec postgres createdb -U postgres bulletin_board

# Или создать локально (если PostgreSQL на хосте)
createdb -U postgres bulletin_board
```

**Проблема: "Connection refused" при подключении к localhost:5432**

Решение:
```bash
# Убедитесь что PostgreSQL работает в Docker
docker-compose ps postgres

# Если нет, запустите
docker-compose up -d postgres

# Если используете PostgreSQL локально, убедитесь что он запущен
pg_isready -h localhost -p 5432
```

### Проверка что всё работает

**Docker стек:**
```bash
# 1. Проверить статус контейнеров
docker-compose ps
# Все должны быть Up (или Exited для db-migrator, это нормально)

# 2. Проверить логи DbMigrator
docker-compose logs db-migrator | grep "Migrations applied"
# Должно быть: "✅ Migrations applied successfully!"

# 3. Проверить логи API
docker-compose logs api | grep "listening"
# Должно быть: "Now listening on: http://[::]:8080"

# 4. Проверить БД
docker-compose exec postgres psql -U postgres -d bulletin_board -c "\dt"
# Должно быть 7 таблиц (including __EFMigrationsHistory)

# 5. Проверить API через curl
curl -X GET http://localhost:8080/api/categories
# Должен вернуть JSON ответ
```

**Локальный запуск:**
```bash
# 1. Проверить что PostgreSQL работает
docker-compose ps postgres

# 2. Проверить что DbMigrator завершился успешно
# (в терминале должно быть: "✅ Migrations applied successfully!")

# 3. Проверить что API запущен
curl -X GET http://localhost:5000/api/categories
# Должен вернуть JSON ответ

# 4. Открыть Swagger
# http://localhost:5000/swagger
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