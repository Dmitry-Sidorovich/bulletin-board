namespace BulletinBoard.Contracts.Errors;

/// <summary>
/// Стандартизированный формат ошибки API (RFC 7807 Problem Details).
/// </summary>
public class ErrorResponse
{
    /// <summary>
    /// HTTP статус код ошибки.
    /// </summary>
    public int Status { get; init; }

    /// <summary>
    /// Краткое описание типа ошибки (например, "Not Found", "Validation Error").
    /// </summary>
    public required string Title { get; init; }

    /// <summary>
    /// Детальное сообщение об ошибке.
    /// </summary>
    public string? Detail { get; init; }

    /// <summary>
    /// Уникальный идентификатор запроса для трассировки.
    /// </summary>
    public required string TraceId { get; init; }

    /// <summary>
    /// Временная метка возникновения ошибки (ISO 8601).
    /// </summary>
    public DateTime Timestamp { get; init; } = DateTime.UtcNow;

    /// <summary>
    /// Ошибки валидации (опционально, для ValidationException).
    /// Ключ — имя поля, значение — массив сообщений об ошибках.
    /// </summary>
    public IDictionary<string, string[]>? Errors { get; init; }
}