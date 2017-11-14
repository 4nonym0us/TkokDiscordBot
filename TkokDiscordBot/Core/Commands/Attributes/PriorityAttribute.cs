using System;

namespace TkokDiscordBot.Core.Commands.Attributes
{
    [AttributeUsage(AttributeTargets.Class)]
    public class PriorityAttribute : Attribute
    {
        /// <summary>
        /// Priority for command handling. Highest prio command handler gets executed first. Default: Medium
        /// </summary>
        public CommandPriority Priority { get; set; }

        public PriorityAttribute(CommandPriority priority)
        {
            Priority = priority;
        }
    }
}
