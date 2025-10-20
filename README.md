# 🎯 Bulletin Board (Электронная доска объявлений)

Веб-приложение для размещения объявлений с поддержкой категорий, поиска и загрузки файлов.

## 📋 Содержание

- [Технологии](#технологии)
- [Архитектура](#архитектура)
- [Быстрый старт](#быстрый-старт)
- [API Endpoints](#api-endpoints)
- [Тестирование](#тестирование)

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

### Вариант 1: Запуск с Docker 🐳
```bash
# 1. Клонировать репозиторий
git clone https://github.com/Dmitry-Sidorovich/bulletin-board.git
cd bulletin-board

# 2. Запустить через Docker Compose
docker-compose up -d

# 3. API доступен по адресу:
# http://localhost:5000
# Swagger UI: http://localhost:5000/swagger
```

### Вариант 2: Локальный запуск
```bash
# 1. Клонировать репозиторий
git clone https://github.com/Dmitry-Sidorovich/bulletin-board.git
cd bulletin-board

# 2. Создать БД PostgreSQL
createdb bulletin_board

# 3. Обновить строку подключения
# В appsettings.Development.json:
# "ConnectionStrings": {
#   "DefaultConnection": "Host=localhost;Port=5432;Database=bulletin_board;Username=your_user;Password=your_password"
# }

# 4. Применить миграции
cd src/BulletinBoard/Hosts/BulletinBoard.Hosts.DbMigrator
dotnet run

# 5. Запустить API
cd ../BulletinBoard.Hosts.Api
dotnet run

# 6. Открыть Swagger
# http://localhost:5000/swagger
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
```bash
# Запустить все тесты
dotnet test

# С покрытием
dotnet test --collect:"XPlat Code Coverage"
```

**Покрытие тестами:**
- Domain: 99%
- Application: 85%
- Всего: 221 тест

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

Проект разработан в рамках курса .NET Full-Stack Developer от СоларЛаб.