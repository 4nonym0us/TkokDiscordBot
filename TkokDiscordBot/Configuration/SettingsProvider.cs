using System;
using System.Globalization;
using System.IO;
using System.Text.Json;

namespace TkokDiscordBot.Configuration;

public static class SettingsProvider
{
    private const string EnvPrefix = "TDB_";
    private const string UserSecretsFileName = "user-secrets.json";

    public static ISettings GetSettings()
    {
        var path = Path.Combine(Directory.GetCurrentDirectory(), UserSecretsFileName);

        if (!File.Exists(path))
        {
            throw new FileNotFoundException($"Unable to locate \"{UserSecretsFileName}\". Ensure that it is present and accessible.", path);
        }

        using var stream = File.OpenRead(path);
        var settings = JsonSerializer.Deserialize(stream, SettingsJsonContext.Default.Settings) ??
                       throw new InvalidOperationException("User secrets deserialized to null.");

        ApplyEnvironmentOverrides(settings);

        return settings;
    }

    private static void ApplyEnvironmentOverrides(Settings settings)
    {
        if (GetEnv(nameof(ISettings.DiscordToken)) is { } token)
        {
            settings.DiscordToken = token;
        }

        if (TryGetEnvUlong(nameof(ISettings.BotCommandsChannelId), out var botChannelId))
        {
            settings.BotCommandsChannelId = botChannelId;
        }

        if (TryGetEnvUlong(nameof(ISettings.HoneypotChannelId), out var honeypotChannelId))
        {
            settings.HoneypotChannelId = honeypotChannelId;
        }

        if (TryGetEnvUlong(nameof(ISettings.MainServerId), out var mainServerId))
        {
            settings.MainServerId = mainServerId;
        }
    }

    private static string? GetEnv(string key) => Environment.GetEnvironmentVariable(EnvPrefix + key);

    private static bool TryGetEnvUlong(string key, out ulong value)
    {
        value = 0;

        return GetEnv(key) is { } raw &&
               !string.IsNullOrWhiteSpace(raw) &&
               ulong.TryParse(raw, NumberStyles.None, CultureInfo.InvariantCulture, out value);
    }
}