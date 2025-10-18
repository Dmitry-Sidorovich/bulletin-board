namespace BulletinBoard.Contracts.Advertisements;

/// <summary>
/// Статус объявления в системе.
/// </summary>
public enum AdStatusDto
{
    /// <summary> Черновик (не опубликовано).</summary>
    Draft = 0,
    
    /// <summary> Опубликовано и видно в поиске.</summary>
    Published = 1,
    
    /// <summary> Снято с публикации (архив).</summary>
    Archived = 2,
    
    /// <summary> Заблокировано админом.</summary>
    Blocked = 3,
}