using BulletinBoard.Domain.Base;
using BulletinBoard.Domain.Enums;
using BulletinBoard.Domain.ValueObjects;

namespace BulletinBoard.Domain.Entities;

/// <summary>
/// Объявление.
/// </summary>
public class Advertisement : EntityBase
{
    /// <summary> Максимально допустимая длина заголовка объявления./// </summary>
    private const int MaxTitleLength = 200;
    
    /// <summary> Максимально допустимая длина описания объявления./// </summary>
    private const int MaxDescriptionLength = 1000;
    
    /// <summary>
    /// Заголовок объявления (обязателен).
    /// </summary>
    public string Title { get; private set; } = string.Empty;
    
    /// <summary>
    /// Текст объявления. Может быть пустым, нормализуется (Trim).
    /// </summary>
    public string Description { get; private set; } = string.Empty;
    
    /// <summary>Цена товара/услуги (может быть 0 для "Даром").</summary>
    public decimal Price { get; private set; }
    
    /// <summary>
    /// Идентификатор категории (FK), к которой относится объявление.
    /// </summary>
    public Guid CategoryId { get; private set; }
    
    /// <summary> Автор объявления (пользователь, FK).</summary>
    public Guid AuthorId { get; private set; }
    
    /// <summary> Контактные данные для этого объявления.</summary>
    public Contact Contact { get; private set; } = default!;
    
    /// <summary> Текущий статус объявления.</summary>
    public AdStatus Status { get; private set; } = AdStatus.Draft;

    /// <summary>
    /// Пустой конструктор для EF Core.
    /// </summary>
    private Advertisement() { }

    /// <summary>
    /// Создаёт корректное объявление.
    /// </summary>
    /// <param name="title"> Заголовок (обязателен, Trim).</param>
    /// <param name="description"> Описание (может быть пустым, Trim).</param>
    /// <param name="price">Цена (может быть 0 для "Даром").</param>
    /// <param name="categoryId"> Категория (обязательна, не может быть Guid.Empty).</param>
    /// <param name="authorId"> Автор/владелец (обязателен, не Guid.Empty).</param>
    /// <param name="contact"> Контактный «снимок» (обязателен).</param>
    /// <param name="status"> Начальный статус (по умолчанию Draft).</param>
    /// <exception cref="ArgumentException"> Если обязательные параметры равны null или нарушены инварианты.</exception>
    public Advertisement(
        string title,
        string? description,
        decimal price,
        Guid categoryId,
        Guid authorId,
        Contact contact,
        AdStatus status = AdStatus.Draft)
    {
        if (string.IsNullOrWhiteSpace(title))
        {
            throw new ArgumentException("Заголовок обязателен.", nameof(title));
        }
        if (title.Trim().Length > MaxTitleLength)
        {
            throw new ArgumentException($"Заголовок не может превышать {MaxTitleLength} символов.", nameof(title));
        }
        if (price < 0)
        {
            throw new ArgumentException("Цена не может быть отрицательной.", nameof(price));
        }
        if (categoryId == Guid.Empty)
        {
            throw new ArgumentException("Категория обязательна.", nameof(categoryId));
        }
        if (authorId == Guid.Empty)
        {
            throw new ArgumentException("Продавец обязателен.", nameof(authorId));
        }
        
        Contact = contact ?? throw new ArgumentNullException(nameof(contact));
        Title = title.Trim();
        Price = price;
        
        var normalizedDescription = (description ?? string.Empty).Trim();
        if (normalizedDescription.Length > MaxDescriptionLength)
        {
            throw new ArgumentException($"Описание не может превышать {MaxDescriptionLength} символов.", nameof(description));
        }
        
        Description = normalizedDescription;
        CategoryId = categoryId;
        AuthorId = authorId;
        Status = status;
    }
    
    /// <summary>
    /// Обновляет текст объявления.
    /// </summary>
    /// <param name="newTitle">Новый заголовок (обязателен).</param>
    /// <param name="newDescription">Новый текст (может быть пустым).</param>
    /// <param name="newPrice">Новая цена (не может быть отрицательной).</param>
    /// <exception cref="ArgumentException">Если <paramref name="newTitle"/> пустой.</exception>
    public void UpdateText(string newTitle, string? newDescription, decimal newPrice)
    {
        if (string.IsNullOrWhiteSpace(newTitle))
        {
            throw new ArgumentException("Заголовок обязателен.", nameof(newTitle));
        }
        if (newTitle.Trim().Length > MaxTitleLength)
        {
            throw new ArgumentException($"Заголовок не может превышать {MaxTitleLength} символов.", nameof(newTitle));
        }
        if (newPrice < 0)
        {
            throw new ArgumentException("Цена не может быть отрицательной.", nameof(newPrice));
        }
        
        Title = newTitle.Trim();
        Price = newPrice;
        var normalizedDescription = (newDescription ?? string.Empty).Trim();
        if (normalizedDescription.Length > MaxDescriptionLength)
        {
            throw new ArgumentException($"Описание не может превышать {MaxDescriptionLength} символов.", nameof(newDescription));
        }

        Description = normalizedDescription;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Меняет категорию объявления.
    /// </summary>
    /// <param name="newCategoryId">Идентификатор новой категории.</param>
    /// <exception cref="ArgumentException">Если <paramref name="newCategoryId"/> пустой.</exception>
    public void ChangeCategory(Guid newCategoryId)
    {
        if (newCategoryId == Guid.Empty)
        {
            throw new ArgumentException("Категория обязательна.", nameof(newCategoryId));
        }

        CategoryId = newCategoryId;
        UpdatedAt = DateTimeOffset.UtcNow;
    }

    /// <summary>
    /// Обновляет контактный «снимок» (например, указать другой номер для этого объявления).
    /// </summary>
    public void UpdateContact(Contact newContact)
    {
        Contact = newContact ?? throw new ArgumentNullException(nameof(newContact));
        UpdatedAt = DateTimeOffset.UtcNow;
    }
    
    /// <summary>
    /// Меняет статус.
    /// </summary>
    public void ChangeStatus(AdStatus newStatus)
    {
        Status = newStatus;
        UpdatedAt = DateTimeOffset.UtcNow;
    }
}