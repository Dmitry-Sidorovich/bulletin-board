namespace BulletinBoard.Application.Exceptions;

/// <summary>
/// Исключение валидации бизнес-правил.
/// Используется для ошибок, не связанных с FluentValidation (доменная валидация).
/// </summary>
public class ValidationException : Exception
{
    /// <summary>
    /// Словарь ошибок валидации (поле → сообщение об ошибке).
    /// </summary>
    public IDictionary<string, string[]> Errors { get; }

    /// <summary>
    /// Создает исключение валидации с общим сообщением.
    /// Используется для простых случаев без привязки к конкретному полю.
    /// </summary>
    /// <param name="message">Сообщение об ошибке.</param>
    public ValidationException(string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>();
    }
    
    /// <summary>
    /// Создает исключение валидации для конкретного поля.
    /// </summary>
    /// <param name="field">Имя поля, в котором произошла ошибка.</param>
    /// <param name="message">Сообщение об ошибке.</param>
    public ValidationException(string field, string message) : base(message)
    {
        Errors = new Dictionary<string, string[]>
        {
            [field] = new[] { message }
        };
    }

    /// <summary>
    /// Создает исключение валидации с несколькими ошибками.
    /// </summary>
    public ValidationException(IDictionary<string, string[]> errors)
        : base("Произошла одна или несколько ошибок валидации.")
    {
        Errors = errors;
    }
}