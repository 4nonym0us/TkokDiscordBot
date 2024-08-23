using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using JetBrains.Annotations;

namespace TkokDiscordBot.Core.CommandsNext;

[Hidden]
[UsedImplicitly]
public class PingCommand : BaseCommandModule
{
    [Command("ping")]
    public async Task PingAsync(CommandContext context)
    {
        await context.RespondAsync($"!pong. WS Latency: {context.Client.Ping} ms.");
    }
}