using System.Collections.Generic;

namespace TkokDiscordBot.Configuration;

public interface ISettings
{
    /// <summary>
    /// Discord token
    /// </summary>
    string DiscordToken { get; }

    /// <summary>
    /// Id of #bot-commands channel
    /// </summary>
    ulong BotCommandsChannelId { get; }

    /// <summary>
    /// Id of #bot-commands channel
    /// </summary>
    ulong MainServerId { get; }
}

public sealed class Settings : ISettings
{
    public string DiscordToken { get; set; } = string.Empty;
    public ulong BotCommandsChannelId { get; set; }
    public ulong MainServerId { get; set; }
}