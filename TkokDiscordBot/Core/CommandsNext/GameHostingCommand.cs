using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.EntGaming;

namespace TkokDiscordBot.Core.CommandsNext
{
    internal class GameHostingCommand : ICommandNext
    {
        private readonly EntClient _entClient;

        public GameHostingCommand(EntClient entClient)
        {
            _entClient = entClient;
        }

        [Command("hhost")]
        public async Task Host(CommandContext context, params string[] args)
        {
            if (args.Length == 0)
            {
                return;
            }

            var owner = args[0];
            var ownerValidator = new Regex(@"^([0-9a-zA-Z-_]{3,32})$");
            if (!ownerValidator.IsMatch(owner))
            {
                return;
            }

            var location = "europe";
            if (args.Length == 2)
            {
                var locationRegex = new Regex(@"^(atlanta|ny|la|europe|au|jp|sg)$");
                if (locationRegex.IsMatch(args[1]))
                {
                    location = locationRegex.Match(args[1]).Result("$1");
                }
            }

            var entResponse = await _entClient.HostGame(owner, location);

            await context.RespondAsync(entResponse.ToMdFormat());
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
