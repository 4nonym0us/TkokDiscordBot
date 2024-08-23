using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using JetBrains.Annotations;
using TkokDiscordBot.Analysis;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.Search;

namespace TkokDiscordBot.Core.CommandsNext;

[Hidden]
[UsedImplicitly]
public class SearchWizardCommand : SearchWizardCommandBase, IHasCommandUsage
{

    public SearchWizardCommand(IItemAnalysisService analysisService, IFullTextSearchService fullTextSearch) : base(analysisService, fullTextSearch)
    {
    }

    [Command("search-wizard")]
    [Aliases("wizard", "sw")]
    public async Task SearchWizardAsync(CommandContext context)
    {
        var query = await RunSearchWizardAsync(context);

        if (query is null)
        {
            return;
        }

        await SearchAndRespondAsync(context, query, true);
    }

    public CommandInfo GetUsage()
    {
        var info = new CommandInfo
        {
            Command = "`!search-wizard`, `!sw`, `!wizard`",
            Usage = @"Starts a search wizard. Provides a way to search items in an easy and interactive way. Great starting point for beginners.",
            Order = -2
        };

        return info;
    }
}