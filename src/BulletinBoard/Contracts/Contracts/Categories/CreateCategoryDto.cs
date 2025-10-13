namespace BulletinBoard.Contracts.Categories;

/// <summary>
/// DTO для создания категории.
/// </summary>
public sealed class CreateCategoryDto
{
    /// <summary>Название категории.</summary>
    public string Name { get; init; } = string.Empty;
}