using BulletinBoard.Contracts.Files;

namespace BulletinBoard.Contracts.Advertisements;

/// <summary> Объявление (DTO для чтения).</summary>
public sealed class AdvertisementDto
{
    /// <summary> Идентификатор объявления.</summary>
    public Guid Id { get; init; }

    /// <summary> Заголовок объявления.</summary>
    public string Title { get; init; } = string.Empty;

    /// <summary> Описание объявления.</summary>
    public string Description { get; init; } = string.Empty;

    /// <summary> FK на категорию.</summary>
    public Guid CategoryId { get; init; }

    /// <summary> FK на автора.</summary>
    public Guid AuthorId { get; init; }

    /// <summary> Контактные данные для этого объявления.</summary>
    public ContactDto Contact { get; init; } = default!;

    /// <summary> Текущий статус объявления.</summary>
    public AdStatus Status { get; init; }

    /// <summary> Время создания (UTC).</summary>
    public DateTimeOffset CreatedAt { get; init; }
    
    /// <summary>Прикрепленные файлы.</summary>
    public IReadOnlyList<FileInfoDto> Files { get; set; } = Array.Empty<FileInfoDto>();
}