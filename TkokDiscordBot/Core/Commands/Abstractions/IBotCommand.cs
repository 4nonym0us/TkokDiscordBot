using System.Threading.Tasks;
using DSharpPlus.EventArgs;
using TkokDiscordBot.Core.Commands.Dto;

namespace TkokDiscordBot.Core.Commands.Abstractions
{
    internal interface IBotCommand
    {
        Task<bool> Handle(Bot sender, MessageCreateEventArgs eventArgs);
        CommandInfo GetUsage();
    }
}