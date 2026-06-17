using Lucene.Net.QueryParsers.Classic;
using System.Text.RegularExpressions;

namespace TkokDiscordBot.Extensions;

public static class StringExtensions
{
    /// <param name="s"></param>
    extension(string s)
    {
        /// <summary>
        /// Converts a string to sentence case.
        /// </summary>
        /// <returns></returns>
        public string ToSentenceCase()
        {
            var lowerCase = s.ToLower();
            var r = new Regex(@"(^[a-z])|\.\s+(.)", RegexOptions.ExplicitCapture);
            return r.Replace(lowerCase, str => str.Value.ToUpper());
        }

        /// <summary>
        /// Converts a single long word without spaces to title case sentence.
        /// In example, a string `SampleString` would become `Sample String`.
        /// </summary>
        /// <returns></returns>
        public string ToTitleCase()
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
        /// <returns></returns>
        public string AsLuceneTerm()
        {
            return s.Contains(' ') && !s.StartsWith('(') && !s.EndsWith(')')
                ? $"\"{QueryParserBase.Escape(s)}\""
                : QueryParserBase.Escape(s);
        }

        /// <summary>
        /// Converts string to camel case.
        /// Example: `SampleInput` => `sampleInput`
        /// </summary>
        /// <returns></returns>
        public string ToCamelCase()
        {
            return s[..1].ToLower() + s[1..];
        }

        /// <summary>
        /// Indicates whether the specified string is <see langword="null" /> or an empty string ("").
        /// </summary>
        /// <returns>
        /// <see langword="true" /> if the <paramref name="s" /> parameter is <see langword="null" /> or an empty string (""); otherwise, <see langword="false" />.
        /// </returns>
        public bool IsNullOrEmpty() => string.IsNullOrEmpty(s);
    }
}