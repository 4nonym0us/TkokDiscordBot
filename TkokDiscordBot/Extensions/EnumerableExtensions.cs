using System;
using System.Collections.Generic;
using System.Linq;

namespace TkokDiscordBot.Extensions;

public static class EnumerableExtensions
{
    public static IEnumerable<TSource> WhereIf<TSource>(
        this IEnumerable<TSource> source, bool condition, Func<TSource, bool> predicate)
    {
        return condition ? source.Where(predicate) : source;
    }

    public static bool Contains(this IList<string> source, string value, bool ignoreCase)
    {
        var stringComparison = ignoreCase ? StringComparison.CurrentCultureIgnoreCase : StringComparison.InvariantCulture;
        return source.Any(s => string.Equals(s, value, stringComparison));
    }
}