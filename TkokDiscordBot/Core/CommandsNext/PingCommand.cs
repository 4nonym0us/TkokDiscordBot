using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using JetBrains.Annotations;

namespace TkokDiscordBot.Core.CommandsNext;

[Hidden]
[UsedImplicitly]
public class PingHasCommand : BaseCommandModule
{
    [Command("ping")]
    public async Task Ping(CommandContext context)
    {
        await context.RespondAsync($"!pong. WS Latency: {context.Client.Ping} ms.");
    }
}