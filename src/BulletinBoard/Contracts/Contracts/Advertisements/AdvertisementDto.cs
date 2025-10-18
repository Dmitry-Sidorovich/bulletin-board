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
    
    /// <summary>Цена товара/услуги (может быть 0 для "Даром").</summary>
    public decimal Price { get; init; }

    /// <summary> FK на категорию.</summary>
    public Guid CategoryId { get; init; }
    
    /// <summary>Название категории (для удобства отображения).</summary>
    public string? CategoryName { get; set; }

    /// <summary> FK на автора.</summary>
    public Guid AuthorId { get; init; }

    /// <summary> Контактные данные для этого объявления.</summary>
    public ContactDto Contact { get; init; } = default!;

    /// <summary> Текущий статус объявления.</summary>
    public AdStatusDto Status { get; init; }

    /// <summary> Время создания (UTC).</summary>
    public DateTimeOffset CreatedAt { get; init; }
    
    /// <summary>Прикрепленные файлы.</summary>
    public IReadOnlyList<FileInfoDto> Files { get; set; } = Array.Empty<FileInfoDto>();
}