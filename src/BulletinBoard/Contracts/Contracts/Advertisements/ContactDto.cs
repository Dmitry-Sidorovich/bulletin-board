namespace BulletinBoard.Contracts.Advertisements;

/// <summary> Контактные данные, отображаемые в объявлении.</summary>
public sealed class ContactDto
{
    /// <summary> Имя продавца. Обязательно.</summary>
    public string Name { get; init; } = string.Empty;

    /// <summary> Email продавца. Обязательно.</summary>
    public string Email { get; init; } = string.Empty;

    /// <summary> Телефон продавца (опционально).</summary>
    public string? Phone { get; init; }
}