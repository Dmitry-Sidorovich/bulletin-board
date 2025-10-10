using BulletinBoard.Contracts.Common;

namespace BulletinBoard.Application.Common;

/// <summary>
/// Расширения для валидации параметров постраничного вывода.
/// </summary>
public static class PageRequestValidationExtensions
{
    /// <summary>
    /// Бросает исключение, если значения пагинации некорректны.
    /// </summary>
    /// <param name="page">Параметры пагинации.</param>
    /// <param name="maxPageSize">Необязательный верхний предел размера страницы.</param>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Если Page &lt; 1, PageSize &lt;= 0 или PageSize &gt; <paramref name="maxPageSize"/>.
    /// </exception>
    public static void ThrowIfInvalid(this PageRequest page, int? maxPageSize = null)
    {
        if (page.Page < 1)
        {
            throw new ArgumentOutOfRangeException(nameof(page.Page), "Page must be >= 1.");
        }

        if (page.PageSize <= 0)
        {
            throw new ArgumentOutOfRangeException(nameof(page.PageSize), "PageSize must be > 0.");
        }

        if (maxPageSize.HasValue && page.PageSize > maxPageSize.Value)
        {
            throw new ArgumentOutOfRangeException(nameof(page.PageSize), $"PageSize must be <= {maxPageSize.Value}.");
        }
    }
}