using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using JetBrains.Annotations;
using TkokDiscordBot.Analysis;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.Search;

namespace TkokDiscordBot.Core.CommandsNext;

/// <summary>
/// Allows searching items by specified criteria.
/// </summary>
[UsedImplicitly]
public class SearchCommand : SearchWizardCommandBase, IHasCommandUsage
{
    private readonly ISettings _settings;

    public SearchCommand(ISettings settings, IFullTextSearchService fullTextSearch, IItemAnalysisService analysisService)
        : base(analysisService, fullTextSearch)
    {
        _settings = settings;
    }

    [Command("search")]
    [Aliases("s")]
    public async Task SearchAsync(CommandContext context, [RemainingText] string query)
    {
        if (!context.Channel.IsPrivate &&
            context.Channel.Id != _settings.BotCommandsChannelId && context.Guild.Id == _settings.MainServerId)
        {
            var botCommandChannel = context.Guild.GetChannel(_settings.BotCommandsChannelId);
            await context.RespondAsync($"Please use this command in {botCommandChannel.Mention} instead.");
            return;
        }

        if (query is null)
        {
            // Fallback to Search Wizard if no arguments were provided.
            await RunSearchWizardAndRespondAsync(context);
        }
        else
        {
            //Fetch and display the items
            await SearchAndRespondAsync(context, query, false);
        }
    }

    public CommandInfo GetUsage()
    {
        var info = new CommandInfo
        {
            Command = "`!search <query>`, `!s <query>`",
            Order = -1,
            Usage =
                @"Search for items using full-text search. All filters are optional, multiple filters can be combined. Use double quotes to look for exact phrase match. Can search by **name**, **slot**, **type**, **quality**, **boss**, **level**, **class**, **description** or **special**.
│ 
│ **Examples**:
│ `!search Druid` - find items for Druid.
│ `!search ""Narith VX""` – find loot from Narith UVX.
│ `!search Bow` - find Bows.
│ `!search ""Dual Dagger""` - find Dual Daggers.
│ `!s Accessory AND Epic` - find only Epic Accessories.
│ `!s Avnos Karnos` (or `!s Avnos OR Karnos`) - find loot from Avnos & Karnos.
│ `!s ""Phantom Stalker"" AND Chest` - find chests for PS.
│ `!s Warrior AND (Gloves OR Helm)` - find gloves and helmets for Warrior.
│ 
│ **Tips**:
│ Use `!search-wizard` for interactive search experience.
│ Use `!search-guide` for detailed search syntax guide with more examples.
│ Not sure what are possible values for specific property? – Use `!explore`.
"
        };

        return info;
    }
}