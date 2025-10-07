namespace BulletinBoard.Contracts.Advertisements;

/// <summary>
/// Команда смены статуса объявления.
/// </summary>
public sealed class ChangeAdvertisementStatusDto
{
    /// <summary> Новый статус объявления.</summary>
    public AdStatus Status { get; init; }
}