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