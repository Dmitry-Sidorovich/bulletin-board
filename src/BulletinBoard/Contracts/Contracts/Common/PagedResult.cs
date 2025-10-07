namespace BulletinBoard.Contracts.Common;

/// <summary>
/// Результат постраничного вывода: элементы и метаданные.
/// </summary>
/// <typeparam name="T">Тип элементов страницы.</typeparam>
public sealed class PagedResult<T>
{
    /// <summary>Элементы текущей страницы.</summary>
    public IReadOnlyList<T> Items { get; init; } = Array.Empty<T>();

    /// <summary>Номер страницы (начиная с 1).</summary>
    public int Page { get; init; }

    /// <summary>Размер страницы.</summary>
    public int PageSize { get; init; }

    /// <summary>Общее число элементов во всём наборе.</summary>
    public int TotalCount { get; init; }

    /// <summary>Общее число страниц (округление вверх).</summary>
    public int TotalPages =>
        PageSize <= 0 ? 0 : (int)Math.Ceiling((double)TotalCount / PageSize);

    /// <summary>Есть ли предыдущая страница.</summary>
    public bool HasPrevious => Page > 1;

    /// <summary>Есть ли следующая страница.</summary>
    public bool HasNext => Page < TotalPages;
}