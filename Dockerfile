# ============================================
# STAGE 1: Build DbMigrator
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-migrator

WORKDIR /source

COPY ["BulletinBoard.sln", "./"]
COPY ["src/", "./src/"]
COPY ["tests/", "./tests/"]

RUN dotnet restore "src/BulletinBoard/Hosts/BulletinBoard.Hosts.DbMigrator/BulletinBoard.Hosts.DbMigrator.csproj"

RUN dotnet publish "src/BulletinBoard/Hosts/BulletinBoard.Hosts.DbMigrator/BulletinBoard.Hosts.DbMigrator.csproj" \
    -c Release \
    -o /app/migrator/publish \
    --no-restore

# ============================================
# STAGE 2: Build API
# ============================================
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build-api

WORKDIR /source

COPY ["BulletinBoard.sln", "./"]
COPY ["src/", "./src/"]
COPY ["tests/", "./tests/"]

RUN dotnet restore "src/BulletinBoard/Hosts/BulletinBoard.Hosts.Api/BulletinBoard.Hosts.Api.csproj"

RUN dotnet publish "src/BulletinBoard/Hosts/BulletinBoard.Hosts.Api/BulletinBoard.Hosts.Api.csproj" \
    -c Release \
    -o /app/api/publish \
    --no-restore

# ============================================
# STAGE 3: DbMigrator Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS migrator

WORKDIR /app

# Копируем DbMigrator
COPY --from=build-migrator /app/migrator/publish .

# Удаляем Development конфиг, чтобы не переопределил основной
RUN rm -f /app/appsettings.Development.json

# Копируем только Production конфигурацию
# appsettings.json содержит localhost, что не подходит для контейнера
COPY src/BulletinBoard/Hosts/BulletinBoard.Hosts.DbMigrator/appsettings.Production.json ./appsettings.json

# Копируем папку Migrations (для EF Core)
COPY src/BulletinBoard/Hosts/BulletinBoard.Hosts.DbMigrator/Migrations ./Migrations

# Запускаем миграции
ENTRYPOINT ["dotnet", "BulletinBoard.Hosts.DbMigrator.dll"]

# ============================================
# STAGE 4: API Runtime
# ============================================
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS api

WORKDIR /app

# Копируем API
COPY --from=build-api /app/api/publish .

# Создаём директорию для загруженных файлов
RUN mkdir -p wwwroot/uploads

# Expose порт (по умолчанию ASP.NET Core слушает на 8080 в контейнере)
EXPOSE 8080

# Запускаем приложение
ENTRYPOINT ["dotnet", "BulletinBoard.Hosts.Api.dll"]
