using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;

namespace TkokDiscordBot.Core.Commands.Abstractions;

public interface IBotCommand : IHasCommandUsage
{
    Task<bool> HandleAsync(DiscordClient client, MessageCreateEventArgs args);
}