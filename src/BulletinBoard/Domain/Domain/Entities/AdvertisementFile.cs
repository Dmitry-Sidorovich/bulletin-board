using BulletinBoard.Domain.Base;

namespace BulletinBoard.Domain.Entities;

/// <summary>
/// Связь между объявлением и файлом (промежуточная таблица).
/// Позволяет прикреплять несколько файлов к одному объявлению.
/// </summary>
public class AdvertisementFile : EntityBase
{
    /// <summary>
    /// ID объявления.
    /// </summary>
    public Guid AdvertisementId { get; private set; }

    /// <summary>
    /// ID файла.
    /// </summary>
    public Guid FileId { get; private set; }

    /// <summary>
    /// Порядок отображения (0 = главное фото).
    /// </summary>
    public int Order { get; private set; }

    /// <summary>
    /// Конструктор для EF Core.
    /// </summary>
    private AdvertisementFile() { }

    /// <summary>
    /// Создает связь между объявлением и файлом.
    /// </summary>
    /// <param name="advertisementId">ID объявления.</param>
    /// <param name="fileId">ID файла.</param>
    /// <param name="order">Порядок отображения.</param>
    public AdvertisementFile(Guid advertisementId, Guid fileId, int order = 0)
    {
        if (advertisementId == Guid.Empty)
        {
            throw new ArgumentException("ID объявления обязателен.", nameof(advertisementId));
        }
        if (fileId == Guid.Empty)
        {
            throw new ArgumentException("ID файла обязателен.", nameof(fileId));
        }
        if (order < 0)
        {
            throw new ArgumentException("Порядок не может быть отрицательным.", nameof(order));
        }

        AdvertisementId = advertisementId;
        FileId = fileId;
        Order = order;
    }

    /// <summary>
    /// Изменить порядок отображения.
    /// </summary>
    public void ChangeOrder(int newOrder)
    {
        if (newOrder < 0)
        {
            throw new ArgumentException("Порядок не может быть отрицательным.", nameof(newOrder));
        }
        Order = newOrder;
    }
}