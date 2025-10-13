namespace BulletinBoard.Application.Exceptions;

/// <summary>
/// Исключение, выбрасываемое когда запрашиваемая сущность не найдена.
/// </summary>
public class NotFoundException : Exception
{
    /// <summary>
    /// Идентификатор не найденной сущности.
    /// </summary>
    public string EntityId { get; }

    /// <summary>
    /// Тип сущности (например, "Advertisement", "Category").
    /// </summary>
    public string EntityType { get; }

    /// <summary>
    /// Создает исключение для не найденной сущности.
    /// </summary>
    /// <param name="entityType">Тип сущности.</param>
    /// <param name="entityId">Идентификатор сущности.</param>
    public NotFoundException(string entityType, string entityId)
        : base($"{entityType} с ID '{entityId}' не найден.")
    {
        EntityType = entityType;
        EntityId = entityId;
    }

    /// <summary>
    /// Создает исключение для не найденной сущности с кастомным сообщением.
    /// </summary>
    public NotFoundException(string entityType, string entityId, string message)
        : base(message)
    {
        EntityType = entityType;
        EntityId = entityId;
    }
}