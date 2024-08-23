using TkokDiscordBot.Core.Commands.Dto;

namespace TkokDiscordBot.Core.Commands.Abstractions
{
    /// <summary>
    /// All classes implement this interface are automatically registered as a commands
    /// </summary>
    internal interface ICommandNext
    {
        CommandInfo GetUsage();
    }
}