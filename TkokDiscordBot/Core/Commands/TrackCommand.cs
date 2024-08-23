using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.EntGaming;
using TkokDiscordBot.EntGaming.Dto;

namespace TkokDiscordBot.Core.Commands
{
    internal class TrackCommand : IBotCommand
    {
        private readonly EntClient _entClient;

        public TrackCommand(EntClient entClient)
        {
            _entClient = entClient;
        }

        public async Task<bool> Handle(Bot sender, MessageCreateEventArgs eventArgs)
        {
            var message = eventArgs.Message.Content;

            var commandRegex = new Regex(@"^!track (.*?)$").Match(message.ToLower().Trim());
            if (!commandRegex.Success)
            {
                return false;
            }

            var gameNameOrCommand = commandRegex.Result("$1").Trim();

            if (!string.IsNullOrWhiteSpace(gameNameOrCommand))
            {
                if (gameNameOrCommand == "disable")
                {
                    _entClient.GameInfo = null;
                    await eventArgs.Message.RespondAsync(":heavy_multiplication_x: Disabling game tracking.");
                }
                else
                {
                    _entClient.GameInfo = new LobbyStatus(gameNameOrCommand);
                    await eventArgs.Message.RespondAsync($":eye: Monitoring game `{gameNameOrCommand}`.");
                }
            }

            return true;
        }

        public CommandInfo GetUsage()
        {
            var info = new CommandInfo();
            info.Command = "!track <gamename>";
            info.Usage = "Tracks a game on `EntGaming.Net` by game name. Lobby info will be displayed in the Topic of main channel.";
            return info;
        }
    }
}
