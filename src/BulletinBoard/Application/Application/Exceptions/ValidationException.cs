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
    /// Создает исключение валидации с одной ошибкой.
    /// </summary>
    public ValidationException(string field, string message)
        : base("Произошла ошибка валидации.")
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