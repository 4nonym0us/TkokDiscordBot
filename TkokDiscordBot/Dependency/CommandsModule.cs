using Autofac;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Extensions;

namespace TkokDiscordBot.Dependency
{
    internal class CommandsModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterCommands<IBotCommand>();
            builder.RegisterCommands<ICommandNext>();
        }
    }
}
