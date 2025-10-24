namespace BulletinBoard.Infrastructure.Contexts.Auth.Options;

/// <summary>
/// Параметры конфигурации JWT (JSON Web Token) аутентификации.
/// </summary>
/// <remarks>
/// Эти параметры читаются из файла конфигурации (appsettings.json или переменных окружения)
/// и используются для генерации и валидации JWT токенов.
/// Обычно привязываются к секции "Jwt" в конфигурации.
/// </remarks>
public sealed class JwtOptions
{
    /// <summary>
    /// Издатель токена - организация/приложение, которое выпустило токен.
    /// </summary>
    /// <remarks>
    /// Используется при валидации токена для проверки его происхождения.
    /// Пример: "BulletinBoardApi"
    /// </remarks>
    public string Issuer { get; init; } = "";

    /// <summary>
    /// Аудитория токена - приложение/сервис, для которого предназначен токен.
    /// </summary>
    /// <remarks>
    /// Используется при валидации токена для проверки его целевого назначения.
    /// Пример: "BulletinBoardWebApp"
    /// </remarks>
    public string Audience { get; init; } = "";

    /// <summary>
    /// Секретный ключ для подписи токена (HMAC256).
    /// </summary>
    /// <remarks>
    /// ВАЖНО: Минимальная длина 32 символа для алгоритма HMAC256.
    /// Должен храниться в защищённом месте (переменные окружения, Key Vault, etc.).
    /// НИКОГДА не коммитить в git репозиторий!
    /// </remarks>
    public string Secret { get; init; } = "";

    /// <summary>
    /// Время жизни access token в минутах.
    /// </summary>
    /// <remarks>
    /// Access token используется для доступа к защищённым ресурсам.
    /// Короткое время жизни (обычно 15-60 минут) повышает безопасность.
    /// По умолчанию: 60 минут.
    /// </remarks>
    public int AccessTokenExpirationMinutes { get; init; } = 60;

    /// <summary>
    /// Время жизни refresh token в днях.
    /// </summary>
    /// <remarks>
    /// Refresh token используется для получения нового access token без повторной аутентификации.
    /// Обычно имеет более длительное время жизни (7-30 дней).
    /// По умолчанию: 7 дней.
    /// </remarks>
    public int RefreshTokenExpirationDays { get; init; } = 7;
}