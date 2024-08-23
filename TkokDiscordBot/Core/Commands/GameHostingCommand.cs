using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.EntGaming;

namespace TkokDiscordBot.Core.Commands
{
    internal class GameHostingCommand : IBotCommand
    {
        private readonly EntClient _entClient;

        public GameHostingCommand(EntClient entClient)
        {
            _entClient = entClient;
        }

        public async Task<bool> Handle(DiscordClient sender, MessageCreateEventArgs eventArgs)
        {
            var message = eventArgs.Message.Content;

            var commandRegex = new Regex(@"^!host ([0-9a-zA-Z-_]){3,32}\s?(atlanta|ny|la|europe|au|jp|sg)?$").Match(message.ToLower().Trim());
            if (!commandRegex.Success)
            {
                return false;
            }

            var owner = commandRegex.Result("$1").Trim();
            var location = commandRegex.Groups.Count == 2 ? commandRegex.Result("$2").Trim() : "europe";

            var entResponse = await _entClient.HostGame(owner, location);

            await eventArgs.Message.RespondAsync(entResponse.ToMdFormat());

            return true;
        }

        public CommandInfo GetUsage()
        {
            var info = new CommandInfo();
            info.Command = "!host <owner> [region]";
            info.Usage = "Host a game on Ent. Default region: **europe**. Available regions: **atlanta**, **ny**, **la**, **europe**, **au**, **jp**, **sg**.";
            return info;
        }
    }
}
