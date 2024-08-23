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
        /// Username on Entgaming.com
        /// </summary>
        string EntUsername { get; set; }

        /// <summary>
        /// Password on Entgaming.com
        /// </summary>
        string EntPassword { get; set; }

        [Option(DefaultValue = ":824bv")]
        string EntMap { get; set; }

        /// <summary>
        /// User Ids, separate multiple id's with comma
        /// </summary>
        string DiscordBotAdmins { get; set; }

        /// <summary>
        /// User Ids, separate multiple id's with comma
        /// </summary>
        long MainChannelId { get; set; }
    }
}