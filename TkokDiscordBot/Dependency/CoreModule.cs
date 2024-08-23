using Autofac;
using Config.Net;
using JetBrains.Annotations;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Data;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Extensions;

namespace TkokDiscordBot.Dependency;

[UsedImplicitly]
internal class CoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PasteBinItemsLoader>().As<IItemsLoader>().InstancePerDependency(); //PasteBinItemsLoader|LocalFileItemsLoader
        builder.RegisterType<ItemsStore>().As<IItemsStore>().SingleInstance();
        builder.RegisterType<ItemsRepository>().As<IItemsRepository>().InstancePerDependency();
        builder.RegisterType<Bot>().AsSelf().SingleInstance();

        var settings = new ConfigurationBuilder<ISettings>()
            .UseAppConfig()
            .UseJsonFile("user-secrets.json")
            .Build();

        builder.Register(_ => settings).As<ISettings>().SingleInstance();

        builder.RegisterCommands<IBotCommand>();
        builder.RegisterCommands<IHasCommandUsage>();
    }
}