using Autofac;
using Config.Net;
using JetBrains.Annotations;
using TkokDiscordBot.Analysis;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Data;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Extensions;
using TkokDiscordBot.Search;

namespace TkokDiscordBot.Dependency;

[UsedImplicitly]
internal class CoreModule : Module
{
    protected override void Load(ContainerBuilder builder)
    {
        builder.RegisterType<PasteBinItemsLoader>() // PasteBinItemsLoader | LocalFileItemsLoader
            .As<IItemsLoader>()
            .InstancePerDependency();

        builder.RegisterType<ItemsStore>()
            .As<IItemsStore>()
            .SingleInstance();

        builder.RegisterType<ItemAnalysisService>()
            .As<IItemAnalysisService>()
            .InstancePerDependency();
        
        builder.RegisterType<ItemsRepository>()
            .As<IItemsRepository>()
            .InstancePerDependency();

        builder.RegisterType<FullTextSearchService>()
            .As<IFullTextSearchService>()
            .SingleInstance();

        builder.RegisterType<Bot>().AsSelf().SingleInstance();

        var settings = new ConfigurationBuilder<ISettings>()
            .UseAppConfig()
            .UseJsonFile("user-secrets.json")
            .Build();

        builder.Register(_ => settings)
            .As<ISettings>()
            .SingleInstance();

        builder.RegisterCommands<IBotCommand>();
        builder.RegisterCommands<IHasCommandUsage>();
    }
}