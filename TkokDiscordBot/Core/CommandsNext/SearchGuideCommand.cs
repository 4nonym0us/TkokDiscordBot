using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using JetBrains.Annotations;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;

namespace TkokDiscordBot.Core.CommandsNext;

/// <summary>
/// Displays a list of possible values of fields, which are used for searching items.
/// </summary>
[UsedImplicitly]
public class SearchGuideCommand : BaseCommandModule, IHasCommandUsage
{
    [Command("search-guide")]
    [Aliases("sg")]
    public async Task SearchGuideAsync(CommandContext context)
    {
        var builder = new DiscordEmbedBuilder();

        builder.WithDescription("*Skip to examples section below if you just want to see how it works.*");

        builder.AddField("📚 Guide 📚", @"
• A Single Term is a single word such as `!s Bow` or `!s Ranger`.
• A Phrase is a group of words surrounded by double quotes such as `!s ""Dual axe""`.
• Multiple words *without double quotes* are considered as **multiple Single Terms**.
• The **OR** operator is the default conjunction operator (`!s Mail Mithril` is equal to `!s Mail OR Mithril`).
• Search is case-insensitive.
• Multiple terms can be combined together with Boolean operators (`AND`/`OR`) to form a more complex query.");

        builder.AddField("⚠ Hints ⚠", @"
In order to search for specific **phrase**, surround the phrase with **double quotes** (`""`) like this: `!s ""Dual axe""`.
✅ Good: `!s Ranger AND Boots` – finds all Boots that can be equipped by Ranger.
❌ Bad:  `!s Ranger Boots` (equal to `!s Ranger OR Boots`) – finds all Boots as well as all items that can be equipped by Ranger.
✅ Good: `!s ""Narith UVX""` – finds all loot from Narith UVX.
❌ Bad:  `!s Narith UVX` (equal to `!s Narith OR UVX`) – finds all loot from Narith and all UVX bosses.");

        builder.AddField("📝 Examples 📝", @"
• `!search Cleric` (equal to `!s Cleric`) – find all items for Cleric.
• `!s ""Phantom Stalker"" AND Gloves` – find all Gloves for Phantom Stalker.
• `!s Mail Gloves Shoulder` (equal to `!s Mail OR Gloves OR Shoulder`) – find all Boots, Gloves, Shoulders.
• `!s ""Narith UVX""` – find all drops from Narith UVX.
• `!s (Naztar Karrix ""Sand Golem"") AND Warrior` – find all items for Cleric from Naztar, Karrix and SG.
• `!s ""Dual Dagger""` – find all dual daggers.
• `!s Mithril Mail (Leather AND Epic)` – find any Mithril, any Mail and only Epic Leather items.
• `!s Artifact AND ""Dual Dagger""` – find all dual daggers artifacts.
• `!s (Broodmother Narith) AND UVX` – find all drops from Broodmother & Narith killed in UVX mode.");

        builder.WithFooter("*Complete syntax guide for nerds: https://lucenenet.apache.org/docs/4.8.0-beta00016/api/queryparser/Lucene.Net.QueryParsers.Classic.html*");

        await context.Message.RespondAsync(builder);
    }

    public CommandInfo GetUsage()
    {
        var info = new CommandInfo
        {
            Command = "`!search-guide`, `!sg`",
            Usage = "Print a detailed guide with a lot of examples on querying syntax for 'search' command.\r\n"
        };
        return info;
    }
}