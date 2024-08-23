namespace TkokDiscordBot.EntGaming.Dto
{
    public class GameHostedResponse
    {
        public string Message { get; set; }

        public string GameName { get; set; }

        public GameHostedResponse(string message, string gameName = null)
        {
            Message = message;
            GameName = gameName;
        }

        public string ToMdFormat()
        {
            return $"{Message} Game name is **__{GameName}__**.";
        }
    }
}
