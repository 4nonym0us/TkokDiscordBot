using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.EntGaming;
using TkokDiscordBot.EntGaming.Dto;

namespace TkokDiscordBot.Core.CommandsNext
{
    internal class TrackCommand : ICommandNext
    {
        private readonly EntClient _entClient;

        public TrackCommand(EntClient entClient)
        {
            _entClient = entClient;
        }

        [Command("track")]
        public async Task Track(CommandContext ctx, string argument)
        {
            var commandRegex = new Regex(@"^(disable|[0-9A-Za-z-_]{3,32})$").Match(argument.Trim());
            if (!commandRegex.Success)
            {
                return;
            }

            var gameNameOrCommand = argument.Trim();

            if (!string.IsNullOrWhiteSpace(gameNameOrCommand))
            {
                if (gameNameOrCommand == "disable")
                {
                    _entClient.GameInfo = null;
                    await ctx.RespondAsync(":heavy_multiplication_x: Disabling game tracking.");
                }
                else
                {
                    _entClient.GameInfo = new LobbyStatus(gameNameOrCommand);
                    await ctx.RespondAsync($":eye: Monitoring game `{gameNameOrCommand}`.");
                }
            }
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
