namespace BulletinBoard.Contracts.Categories;

/// <summary>
/// Категория (DTO для чтения/отдачи наружу).
/// </summary>
public sealed class CategoryDto
{
    /// <summary> Идентификатор категории.</summary>
    public Guid Id { get; init; }
    
    /// <summary> Название категории.</summary>
    public string Name { get; init; } = string.Empty;
    
    /// <summary>
    /// Идентификатор родителя.
    /// Если <c>null</c>, то корневая категория.
    /// </summary>
    public Guid? ParentId { get; init; }
}