using BulletinBoard.Domain.Base;

namespace BulletinBoard.Domain.Entities;

/// <summary>
/// Категория объявлений. Может быть корневой или подкатегорией. (self-reference)
/// </summary>
public class Category : EntityBase
{
    /// <summary>
    /// Максимальная длина имени категории.
    /// </summary>
    private const int MaxNameLength = 100;
    
    /// <summary> Название категории.</summary>
    public string Name { get; private set; } = string.Empty;
    
    /// <summary>
    /// Идентификатор родительской категории.
    /// Если <c>null</c>, то категория верхнего уровня.
    /// </summary>
    public Guid? ParentId { get; private set; }
    
    /// <summary> Пустой конструктор, используемый EF Core при материализации из БД.</summary>
    private Category() {}
    
    /// <summary>
    /// Создаёт категорию. Если <paramref name="parentId"/> = <c>null</c> — это корневая категория.
    /// </summary>
    /// <param name="name">Название категории.</param>
    /// <param name="parentId">Идентификатор родителя (null для корня).</param>
    public Category(string name, Guid? parentId = null)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new ArgumentException("Название категории обязательно.", nameof(name));
        }

        var normalizedName = name.Trim();
        if (normalizedName.Length > MaxNameLength)
        {
            throw new ArgumentException($"Название категории не может превышать {MaxNameLength} символов.", nameof(name));
        }

        Name = normalizedName;
        ParentId = parentId;
    }
}