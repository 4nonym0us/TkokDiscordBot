using Config.Net;

namespace TkokDiscordBot.Configuration
{
    public interface ISettings
    {
        /// <summary>
        /// Discord token
        /// </summary>
        string DiscordToken { get; set; }
        
        /// <summary>
        /// User Ids, separate multiple id's with comma
        /// </summary>
        string DiscordBotAdmins { get; set; }

        /// <summary>
        /// Id of #main channel
        /// </summary>
        ulong MainChannelId { get; set; }
        
        /// <summary>
        /// Id of #media channel
        /// </summary>
        ulong MediaChannelId { get; set; }
        
        /// <summary>
        /// Id of #bot-commands channel
        /// </summary>
        ulong BotCommandsChannelId { get; set; }
    }
}