namespace BulletinBoard.Contracts.Common;

/// <summary>
/// Запрос постраничного вывода.
/// </summary>
public sealed class PageRequest
{
    /// <summary>Номер страницы (начиная с 1).</summary>
    public int Page { get; init; }

    /// <summary>Размер страницы (количество элементов на странице, &gt; 0).</summary>
    public int PageSize { get; init; }
}