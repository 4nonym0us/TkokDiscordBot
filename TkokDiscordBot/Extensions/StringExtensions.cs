using System.Text.RegularExpressions;

namespace TkokDiscordBot.Extensions;

public static class StringExtensions
{
    /// <summary>
    /// Converts a string to sentence case.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string ToSentenceCase(this string s)
    {
        var lowerCase = s.ToLower();
        var r = new Regex(@"(^[a-z])|\.\s+(.)", RegexOptions.ExplicitCapture);
        return r.Replace(lowerCase, str => str.Value.ToUpper());
    }

    /// <summary>
    /// Converts a single long word without spaces to title case sentence.
    /// In example, a string `SampleString` would become `Sample String`.
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string ToTitleCase(this string s)
    {
        return Regex.Replace(s, @"([^^])([A-Z])", @"$1 $2");
    }

    /// <summary>
    /// Ensure that string will be parsed as a single search term in Lucene.
    /// No modifications are done to a string that contains a single word or
    /// when then string is enclosed with braces (i.e. it's a Lucene Group).
    /// If string contains more then one word, it will be surrounded with double quotes.
    /// Examples:
    ///   `Foo` => `Foo` (not modified).
    ///   `Foo Bar` => `"Foo Bar"` (converted to phrase).
    ///   `(Foo AND Bar)` => `(Foo AND Bar)` (input is a Lucene Group, no changes).
    /// </summary>
    /// <param name="s"></param>
    /// <returns></returns>
    public static string AsLuceneTerm(this string s)
    {
        return s.Contains(' ') && !s.StartsWith('(') && !s.EndsWith(')') ? $"\"{s}\"" : s;
    }
}