using System.Text.RegularExpressions;

namespace TkokDiscordBot.Extensions
{
    public static class StringExtensions
    {
        public static string ToSentenceCase(this string s)
        {
            var lowerCase = s.ToLower();
            var r = new Regex(@"(^[a-z])|\.\s+(.)", RegexOptions.ExplicitCapture);
            return r.Replace(lowerCase, str => str.Value.ToUpper());
        }
    }
}
