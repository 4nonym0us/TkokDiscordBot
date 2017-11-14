using Castle.Core.Internal;
using TkokDiscordBot.Configuration;

namespace TkokDiscordBot.Extensions
{
    public static class SettingsExtensions
    {
        public static bool IsEntConfigured(this ISettings settings)
        {
            return !settings.EntMap.IsNullOrEmpty() &&
                   !settings.EntUsername.IsNullOrEmpty() &&
                   !settings.EntPassword.IsNullOrEmpty();
        }
    }
}
