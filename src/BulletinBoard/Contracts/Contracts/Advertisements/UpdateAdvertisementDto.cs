namespace BulletinBoard.Contracts.Advertisements;

/// <summary>
/// Обновление объявления.
/// </summary>
public sealed class UpdateAdvertisementDto
{
    /// <summary> Новый заголовок (обязателен).</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary> Новое описание (может быть пустым).</summary>
    public string? Description { get; init; }

    /// <summary> Новая категория (обязательна).</summary>
    public Guid CategoryId { get; init; }

    /// <summary> Новые контактные данные (обязательны для обновления).</summary>
    public ContactDto Contact { get; init; } = default!;
}