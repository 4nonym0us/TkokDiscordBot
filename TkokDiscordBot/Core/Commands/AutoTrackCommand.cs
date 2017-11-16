using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.EventArgs;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Attributes;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.EntGaming;
using TkokDiscordBot.EntGaming.Dto;
using TkokDiscordBot.Enums;

namespace TkokDiscordBot.Core.Commands
{
    [Priority(CommandPriority.Low)]
    [Cooldown(3, 60, CooldownBucketType.User)]
    internal class AutoTrackCommand : IBotCommand
    {
        private readonly EntClient _entClient;

        public AutoTrackCommand(EntClient entClient)
        {
            _entClient = entClient;
        }

        public async Task<bool> Handle(DiscordClient sender, MessageCreateEventArgs eventArgs)
        {
            var message = eventArgs.Message.Content;

            var commandRegex = new Regex(@"(?:^|\s)([a-z]+\d{2,})(?:\s|$)").Match(message.Trim());
            if (!commandRegex.Success)
            {
                return false;
            }

            var gameName = commandRegex.Result("$1").Trim();

            if (!string.IsNullOrWhiteSpace(gameName))
            {
                _entClient.GameInfo = new LobbyStatus(gameName);

                await eventArgs.Message.RespondAsync($"<:bot:379752914215763994> Detected game name. :eye: Monitoring game `{gameName}`.");
            }


            return true;
        }

        public CommandInfo GetUsage()
        {
            return null;
        }
    }
}
