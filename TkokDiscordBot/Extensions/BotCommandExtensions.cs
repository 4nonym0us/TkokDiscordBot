using Castle.Core.Internal;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Attributes;
using TkokDiscordBot.Enums;

namespace TkokDiscordBot.Extensions
{
    public static class BotCommandExtensions
    {
        public static CommandPriority GetCommandPriority(this IBotCommand command)
        {
            return command.GetType().GetAttribute<PriorityAttribute>()?.Priority ?? CommandPriority.Medium;
        }
    }
}
