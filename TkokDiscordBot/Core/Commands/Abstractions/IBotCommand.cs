using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using TkokDiscordBot.Core.Commands.Dto;

namespace TkokDiscordBot.Core.Commands.Abstractions
{
    internal interface IBotCommand
    {
        Task<bool> Handle(DiscordClient sender, MessageCreateEventArgs eventArgs);
        CommandInfo GetUsage();
    }
}