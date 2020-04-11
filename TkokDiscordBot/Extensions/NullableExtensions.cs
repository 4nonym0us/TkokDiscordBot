namespace TkokDiscordBot.Extensions
{
    public static class NullableExtensions
    {
        public static int? ToNullableInt(this string s)
        {
            if (int.TryParse(s, out var i)) return i;
            return null;
        }
    }
}
