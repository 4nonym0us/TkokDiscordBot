using TkokDiscordBot.Core.Commands.Dto;

namespace TkokDiscordBot.Core.Commands.Abstractions;

/// <summary>
/// Should be implemented by commands that should be displayed in `!help`.
/// </summary>
public interface IHasCommandUsage
{
    CommandInfo GetUsage();
}