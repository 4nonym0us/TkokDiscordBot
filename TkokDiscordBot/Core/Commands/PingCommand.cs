using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;

namespace TkokDiscordBot.Core.Commands
{
    internal class PingCommand : IBotCommand
    {
        public async Task<bool> Handle(DiscordClient sender, MessageCreateEventArgs eventArgs)
        {
            if (eventArgs.Message.Content == "!ping")
            {
                await eventArgs.Message.RespondAsync($"!pong. WS Latency: {sender.Ping} ms.");
                return true;
            }
            return false;
        }

        public CommandInfo GetUsage()
        {
            return null;
        }
    }
}
