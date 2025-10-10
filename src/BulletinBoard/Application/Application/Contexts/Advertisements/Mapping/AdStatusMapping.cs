using ContractsAdStatus = BulletinBoard.Contracts.Advertisements;
using DomainAdStatus = BulletinBoard.Domain.Enums;

namespace BulletinBoard.Application.Contexts.Advertisements.Mapping;

/// <summary>
/// Преобразование статуса объявления между уровнем контрактов (API) и доменной моделью.
/// </summary>
public static class AdStatusMapping
{
    /// <summary>
    /// Преобразует статус объявления из контрактов (API) в доменную модель.
    /// </summary>
    /// <param name="status">Статус из уровня контрактов (<see cref="ContractsAdStatus"/>).</param>
    /// <returns>Доменный статус (<see cref="DomainAdStatus"/>).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Выбрасывается, если передан неизвестный статус.
    /// Это осознанно: при появлении нового значения в контрактах мы сразу получаем жёсткий сигнал.
    /// </exception>
    public static DomainAdStatus.AdStatus ToDomain(this ContractsAdStatus.AdStatus status) => status switch
    {
        ContractsAdStatus.AdStatus.Draft => DomainAdStatus.AdStatus.Draft,
        ContractsAdStatus.AdStatus.Published => DomainAdStatus.AdStatus.Published,
        ContractsAdStatus.AdStatus.Archived => DomainAdStatus.AdStatus.Archived,
        ContractsAdStatus.AdStatus.Blocked => DomainAdStatus.AdStatus.Blocked,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown contracts AdStatus.")
    };
    
    /// <summary>
    /// Преобразует доменный статус объявления в статус уровня контрактов (API).
    /// </summary>
    /// <param name="status">Доменный статус (<see cref="DomainAdStatus"/>).</param>
    /// <returns>Статус из уровня контрактов (<see cref="ContractsAdStatus"/>).</returns>
    /// <exception cref="ArgumentOutOfRangeException">
    /// Выбрасывается, если передан неизвестный статус домена.
    /// </exception>
    public static ContractsAdStatus.AdStatus ToContract(this DomainAdStatus.AdStatus status) => status switch
    {
        DomainAdStatus.AdStatus.Draft => ContractsAdStatus.AdStatus.Draft,
        DomainAdStatus.AdStatus.Published => ContractsAdStatus.AdStatus.Published,
        DomainAdStatus.AdStatus.Archived => ContractsAdStatus.AdStatus.Archived,
        DomainAdStatus.AdStatus.Blocked => ContractsAdStatus.AdStatus.Blocked,
        _ => throw new ArgumentOutOfRangeException(nameof(status), status, "Unknown domain AdStatus.")
    };
}