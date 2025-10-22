# Build stage - компилируем приложение
FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build

WORKDIR /source

# Копируем файлы проекта и весь код
COPY ["BulletinBoard.sln", "./"]
COPY ["src/", "./src/"]
COPY ["tests/", "./tests/"]

# Восстанавливаем зависимости
RUN dotnet restore "BulletinBoard.sln"

# Компилируем приложение (Release конфигурация для оптимизации)
RUN dotnet publish "src/BulletinBoard/Hosts/BulletinBoard.Hosts.Api/BulletinBoard.Hosts.Api.csproj" \
    -c Release \
    -o /app/publish \
    --no-restore

# Runtime stage - создаём финальный image
FROM mcr.microsoft.com/dotnet/aspnet:8.0

WORKDIR /app

# Копируем скомпилированное приложение из build stage
COPY --from=build /app/publish .

# Создаём директорию для загруженных файлов
RUN mkdir -p wwwroot/uploads

# Expose порт (по умолчанию ASP.NET Core слушает на 8080 в контейнере)
EXPOSE 8080

# Запускаем приложение
ENTRYPOINT ["dotnet", "BulletinBoard.Hosts.Api.dll"]
