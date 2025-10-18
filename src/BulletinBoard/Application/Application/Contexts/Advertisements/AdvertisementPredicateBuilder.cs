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

        // Поиск по тексту
        if (!string.IsNullOrWhiteSpace(filter.SearchQuery))
        {
            var query = filter.SearchQuery.ToLower();
            predicate = predicate.And(ad => 
                ad.Title.ToLower().Contains(query) || 
                ad.Description.ToLower().Contains(query));
        }

        // Фильтр по категории
        if (filter.CategoryId.HasValue)
        {
            predicate = predicate.And(ad => ad.CategoryId == filter.CategoryId.Value);
        }

        // Фильтр по цене
        if (filter.MinPrice.HasValue)
        {
            predicate = predicate.And(ad => ad.Price >= filter.MinPrice.Value);
        }

        if (filter.MaxPrice.HasValue)
        {
            predicate = predicate.And(ad => ad.Price <= filter.MaxPrice.Value);
        }

        // Фильтр по дате
        if (filter.CreatedFrom.HasValue)
        {
            predicate = predicate.And(ad => ad.CreatedAt >= filter.CreatedFrom.Value);
        }

        if (filter.CreatedTo.HasValue)
        {
            predicate = predicate.And(ad => ad.CreatedAt <= filter.CreatedTo.Value);
        }

        // Фильтр по статусу
        if (filter.Status.HasValue)
        {
            var domainStatus = filter.Status.Value.ToDomain();
            predicate = predicate.And(ad => ad.Status == domainStatus);
        }

        // Фильтр по автору
        if (filter.AuthorId.HasValue)
        {
            predicate = predicate.And(ad => ad.AuthorId == filter.AuthorId.Value);
        }

        return predicate;
    }

    // Extension method для объединения предикатов
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