namespace BulletinBoard.Contracts.Advertisements;

/// <summary>
/// Параметры поиска и фильтрации объявлений.
/// </summary>
public class AdvertisementFilterDto
{
    /// <summary>Поисковый запрос (по заголовку и описанию).</summary>
    public string? SearchQuery { get; set; }

    /// <summary>Категория (включая подкатегории).</summary>
    public Guid? CategoryId { get; set; }

    /// <summary>Минимальная цена.</summary>
    public decimal? MinPrice { get; set; }

    /// <summary>Максимальная цена.</summary>
    public decimal? MaxPrice { get; set; }

    /// <summary>Дата создания от.</summary>
    public DateTimeOffset? CreatedFrom { get; set; }

    /// <summary>Дата создания до.</summary>
    public DateTimeOffset? CreatedTo { get; set; }

    /// <summary>Статус объявления.</summary>
    public AdStatusDto? Status { get; set; }

    /// <summary>Автор объявления.</summary>
    public Guid? AuthorId { get; set; }

    /// <summary>Сортировка.</summary>
    public AdvertisementSortBy SortBy { get; set; } = AdvertisementSortBy.CreatedAtDesc;

    /// <summary>Номер страницы.</summary>
    public int PageNumber { get; set; } = 1;

    /// <summary>Размер страницы.</summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Варианты сортировки объявлений.
/// </summary>
public enum AdvertisementSortBy
{
    /// <summary>По дате создания (сначала новые).</summary>
    CreatedAtDesc = 0,
    
    /// <summary>По дате создания (сначала старые).</summary>
    CreatedAtAsc = 1,
    
    /// <summary>По цене (по возрастанию).</summary>
    PriceAsc = 2,
    
    /// <summary>По цене (по убыванию).</summary>
    PriceDesc = 3,
    
    /// <summary>По заголовку (A-Z, А-Я).</summary>
    TitleAsc = 4,
    
    /// <summary>По заголовку (Z-A, Я-А).</summary>
    TitleDesc = 5
}