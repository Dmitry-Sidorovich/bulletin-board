namespace BulletinBoard.Domain.Base;

/// <summary>
/// Базовый класс для всех доменных сущностей.
/// </summary>
public abstract class EntityBase
{
    /// <summary> Уникальный идентификатор сущности.</summary>
    public Guid Id { get; protected set; }
    
    /// <summary> Дата и время создания (UTC).</summary>
    public DateTimeOffset CreatedAt { get; protected set; }
    
    /// <summary>Дата и время последнего обновления (UTC).</summary>
    public DateTimeOffset? UpdatedAt { get; protected set; }
    
    /// <summary>
    /// Защищённый конструктор для инициализации базовых полей сущности.
    /// </summary>
    protected EntityBase()
    {
        Id = Guid.NewGuid();
        CreatedAt = DateTimeOffset.UtcNow;
    }
}