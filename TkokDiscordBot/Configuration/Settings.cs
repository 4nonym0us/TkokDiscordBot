namespace TkokDiscordBot.Configuration;

public interface ISettings
{
    /// <summary>
    /// Discord token
    /// </summary>
    string DiscordToken { get; set; }

    /// <summary>
    /// Id of #bot-commands channel
    /// </summary>
    ulong BotCommandsChannelId { get; set; }
}