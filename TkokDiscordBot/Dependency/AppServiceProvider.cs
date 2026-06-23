using System.Collections.Generic;
using Jab;
using TkokDiscordBot.Analysis;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core;
using TkokDiscordBot.Core.Commands;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.CommandsNext;
using TkokDiscordBot.Data;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Search;

namespace TkokDiscordBot.Dependency;

[ServiceProvider(RootServices =
[
    typeof(IEnumerable<IBotCommand>),
    typeof(IEnumerable<IHasCommandUsage>)
])]
[Singleton(typeof(Bot), typeof(Bot))]
[Transient(typeof(IItemsLoader), typeof(PasteBinItemsLoader))]
[Singleton(typeof(IItemsStore), typeof(ItemsStore))]
[Transient(typeof(IItemAnalysisService), typeof(ItemAnalysisService))]
[Transient(typeof(IItemsRepository), typeof(ItemsRepository))]
[Singleton(typeof(IFullTextSearchService), typeof(FullTextSearchService))]
[Singleton(typeof(ISettings), Factory = nameof(SettingsFactory))]
[Transient(typeof(IBotCommand), typeof(GetItemByNameCommand))]
[Transient(typeof(IBotCommand), typeof(HoneypotCommand))]
[Transient(typeof(IHasCommandUsage), typeof(GetItemByNameCommand))]
[Transient(typeof(IHasCommandUsage), typeof(ExploreCommand))]
[Transient(typeof(IHasCommandUsage), typeof(SearchCommand))]
[Transient(typeof(IHasCommandUsage), typeof(SearchGuideCommand))]
[Transient(typeof(IHasCommandUsage), typeof(SearchWizardCommand))]
internal partial class AppServiceProvider
{
    public ISettings SettingsFactory() => SettingsProvider.GetSettings();
}
