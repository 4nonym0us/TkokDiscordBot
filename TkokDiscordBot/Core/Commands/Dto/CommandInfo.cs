namespace TkokDiscordBot.Core.Commands.Dto;

public class CommandInfo
{
    /// <summary>
    /// Command name and aliases.
    /// </summary>
    public string Command { get; set; }

    /// <summary>
    /// Command usage.
    /// </summary>
    public string Usage { get; set; }

    /// <summary>
    /// Priority of the command in the `help` output.
    /// </summary>
    public int Order { get; set; }
}