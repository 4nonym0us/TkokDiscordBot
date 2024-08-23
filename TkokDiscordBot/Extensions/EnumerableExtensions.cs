using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace TkokDiscordBot.Extensions;

public static class EnumerableExtensions
{
    public static IQueryable<TSource> WhereIf<TSource>(
        this IQueryable<TSource> source, bool condition, Expression<Func<TSource, bool>> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }

    public static TSource PickRandom<TSource>(this IList<TSource> source)
    {
        return source[Random.Shared.Next(source.Count())];
    }
}