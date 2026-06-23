using System.Text;

namespace TkokDiscordBot.Core.Commands.Dto;

public class CommandInfo
{
    /// <summary>
    /// Command name and aliases.
    /// </summary>
    public required string Command { get; init; }

    /// <summary>
    /// Command usage.
    /// </summary>
    public required string Usage { get; init; }

    /// <summary>
    /// Priority of the command in the `help` output.
    /// </summary>
    public int Order { get; init; }
}