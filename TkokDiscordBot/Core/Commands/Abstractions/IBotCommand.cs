using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using TkokDiscordBot.Enums;

namespace TkokDiscordBot.Core.Commands.Abstractions;

public interface IBotCommand : IHasCommandUsage
{
    CommandPriority Priority { get; }

    Task<bool> HandleAsync(DiscordClient client, MessageCreateEventArgs args);
}