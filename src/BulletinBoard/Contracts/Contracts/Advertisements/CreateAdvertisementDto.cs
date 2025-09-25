namespace BulletinBoard.Contracts.Advertisements;

/// <summary>
/// Создание объявления.
/// </summary>
public sealed class CreateAdvertisementDto
{
    /// <summary> Заголовок объявления (обязателен).</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary> Описание (необязательно).</summary>
    public string? Description { get; init; }

    /// <summary> Категория (обязательна).</summary>
    public Guid CategoryId { get; init; }

    /// <summary>
    /// Автор объявления (обязателен).
    /// </summary>
    public Guid AuthorId { get; init; }

    /// <summary> Контактные данные — обязательны при создании.</summary>
    public ContactDto Contact { get; init; } = default!;
}