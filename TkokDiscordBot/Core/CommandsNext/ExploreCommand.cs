using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using TkokDiscordBot.Analysis;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;

namespace TkokDiscordBot.Core.CommandsNext;

/// <summary>
/// Displays a list of possible values of fields, which are used for searching items.
/// </summary>
[UsedImplicitly]
public class ExploreCommand : BaseCommandModule, IHasCommandUsage
{
    private readonly IItemAnalysisService _analysisService;

    public ExploreCommand(IItemAnalysisService analysisService)
    {
        _analysisService = analysisService;
    }

    [Command("explore")]
    public async Task ExploreAsync(CommandContext context)
    {
        var itemSourcesPerField = _analysisService.ItemSources.Count / 2;
        var builder = new DiscordEmbedBuilder();
        builder.WithColor(DiscordColor.Azure);
        builder.AddField("Possible `Slot` values", string.Join(", ", _analysisService.ItemSlots), true);
        builder.AddField("Possible `Type` values", string.Join(", ", _analysisService.ItemTypes), true);
        builder.AddField("Possible `Quality` values", string.Join(", ", _analysisService.QualityLevels), true);
        builder.AddField("Available `Class` values", string.Join(", ", _analysisService.AvailableClasses));
        builder.AddField("Possible bosses and other item sources (Part 1)", string.Join(", ", _analysisService.ItemSources.Take(itemSourcesPerField)));
        builder.AddField("Possible bosses and other item sources (Part 2)", string.Join(", ", _analysisService.ItemSources.Skip(itemSourcesPerField)));

        await context.Message.RespondAsync(builder);
    }

    public CommandInfo GetUsage()
    {
        var info = new CommandInfo
        {
            Command = "`!explore`",
            Usage = "Get a list of properties and their values that can be used to search items using 'search' command.\r\n"
        };
        return info;
    }
}