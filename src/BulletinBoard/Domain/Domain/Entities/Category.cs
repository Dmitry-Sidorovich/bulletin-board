using BulletinBoard.Domain.Base;

namespace BulletinBoard.Domain.Entities;

/// <summary>
/// Категория объявлений. Может быть корневой или подкатегорией. (self-reference)
/// </summary>
public class Category : EntityBase
{
    /// <summary> Название категории.</summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Идентификатор родительской категории.
    /// Если <c>null</c>, то категория верхнего уровня.
    /// </summary>
    public Guid? ParentId { get; private set; }
    
    /// <summary>
    /// Родительская категория.
    /// </summary>
    public Category? Parent { get; private set; }
    
    private readonly List<Category> _children = new();
    
    /// <summary>
    /// Дочерние категории.
    /// </summary>
    public IReadOnlyList<Category> Children => _children;
    
    /// <summary> Пустой конструктор, используемый EF Core при материализации из БД.</summary>
    private Category() {}
    
    /// <summary>
    /// Создаёт категорию. Если <paramref name="parentId"/> = <c>null</c> — это корневая категория.
    /// </summary>
    /// <param name="name">Название категории.</param>
    /// <param name="parentId">Идентификатор родителя (null для корня).</param>
    public Category(string name, Guid? parentId = null)
    {
        Name = name;
        ParentId = parentId;
    }
}