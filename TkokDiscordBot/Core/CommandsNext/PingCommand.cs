using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;

namespace TkokDiscordBot.Core.CommandsNext
{
    internal class PingCommand : ICommandNext
    {
        [Command("ping")]
        public async Task Ping(CommandContext context)
        {
            await context.RespondAsync($"!pong. WS Latency: {context.Client.Ping} ms.");
        }

        public CommandInfo GetUsage()
        {
            return null;
        }
    }
}
