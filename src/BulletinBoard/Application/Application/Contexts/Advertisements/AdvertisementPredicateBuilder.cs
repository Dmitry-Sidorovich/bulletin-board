using System.Linq.Expressions;
using BulletinBoard.Application.Contexts.Advertisements.Mapping;
using BulletinBoard.Contracts.Advertisements;
using BulletinBoard.Domain.Entities;

namespace BulletinBoard.Application.Contexts.Advertisements;

/// <summary>
/// Построитель предикатов для фильтрации объявлений.
/// </summary>
public static class AdvertisementPredicateBuilder
{
    /// <summary>
    /// Строит предикат для фильтрации объявлений на основе параметров фильтра.
    /// </summary>
    /// <param name="filter">Параметры фильтрации.</param>
    /// <returns>Выражение для фильтрации объявлений.</returns>
    public static Expression<Func<Advertisement, bool>> Build(AdvertisementFilterDto filter)
    {
        Expression<Func<Advertisement, bool>> predicate = ad => true;

        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
        {
            var query = filter.SearchQuery.ToLower();
            predicate = predicate.And(ad =>
                ad.Title.ToLower().Contains(query) ||
                ad.Description.ToLower().Contains(query));
        }

        if (filter.CategoryId.HasValue)
        {
            predicate = predicate.And(ad => ad.CategoryId == filter.CategoryId.Value);
        }

        if (filter.MinPrice.HasValue)
        {
            predicate = predicate.And(ad => ad.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            predicate = predicate.And(ad => ad.Price <= filter.MaxPrice.Value);
        }

        if (filter.CreatedFrom.HasValue)
        {
            predicate = predicate.And(ad => ad.CreatedAt >= filter.CreatedFrom.Value);
        }

        if (filter.CreatedTo.HasValue)
        {
            predicate = predicate.And(ad => ad.CreatedAt <= filter.CreatedTo.Value);
        }

        if (filter.Status.HasValue)
        {
            var domainStatus = filter.Status.Value.ToDomain();
            predicate = predicate.And(ad => ad.Status == domainStatus);
        }

        if (filter.AuthorId.HasValue)
        {
            predicate = predicate.And(ad => ad.AuthorId == filter.AuthorId.Value);
        }

        return predicate;
    }

    /// <summary>
    /// Объединяет два выражения-предиката оператором AND на уровне Linq To Entities.
    /// Используется для поэтапного построения сложных фильтров.
    /// </summary>
    /// <typeparam name="T">Тип сущности, для которой строится предикат.</typeparam>
    /// <param name="left">Левый предикат.</param>
    /// <param name="right">Правый предикат.</param>
    /// <returns>Скомбинированный предикат (left AND right).</returns>
    /// <remarks>
    /// Использует Expression.Invoke для безопасного объединения предикатов в runtime.
    /// Альтернатива прямому объединению через Expression.AndAlso для более сложных сценариев.
    /// </remarks>
    private static Expression<Func<T, bool>> And<T>(
        this Expression<Func<T, bool>> left,
        Expression<Func<T, bool>> right)
    {
        var parameter = Expression.Parameter(typeof(T));
        var combined = Expression.AndAlso(
            Expression.Invoke(left, parameter),
            Expression.Invoke(right, parameter));
        return Expression.Lambda<Func<T, bool>>(combined, parameter);
    }
}