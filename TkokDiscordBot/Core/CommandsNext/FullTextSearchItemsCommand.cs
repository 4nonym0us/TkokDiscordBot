using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity.Extensions;
using JetBrains.Annotations;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Enums;
using TkokDiscordBot.Extensions;
using TkokDiscordBot.Formatters;

namespace TkokDiscordBot.Core.CommandsNext;

/// <summary>
/// Allows searching items by specified criteria.
/// 
/// Possible slot values: Neck, Ring, Offhand, Belt, Mainhand, Legs, Boots, Shoulder, Helm, Gloves, Chest
/// Possible type values: Treasure, Accessory, Dual axe, Quiver, Shield, Book, Wand, Dual dagger, Idol, Mithril, Axe, Sword, Bow, Dagger, Cloth, Leather, Mace, Mail, Staff
/// Possible quality values: Epic, Enchanted, Relic, Artifact
/// Possible boss values: Any VX Boss, Quest: A Simple,  Easy Task , Broodmother VX, Quest: Proving Your Worth, Broodmother UVX, Narith VX, Narith, Sand Golem, Sand Golem VX, Narith UVX, Sand Golem , Naztar, Karrix, Karrix VX, Sand Golem UVX, Avnos, Karnos, Avnos VX, Karnos VX, Karavnos, Karavnos VX, Quest: Shamanistic Magic, Karrix , Muarki, Muarki VX, Vjaier VX, Karnos , Avnos , Vjaier, Crueltis VX, Crueltis, Ruined Temple Quest, Ruined Temple Outside Zone, Tal'Navi, Tal'Navi VX, Karavnos , Naztar UVX, M'Karsa, M'Karsa VX, Karrix UVX, Inner Temple Offhand Shop, Muarki , Hydra, Hydra VX, Vjaier , Crueltis , Quest: A Simple,  Easy Task  , Avnos UVX , Karnos UVX , Quest: Treasure Hunt, Ortakna, Tal'navi , Ortakna VX, Karavnos UVX , Crypt Fiend , Crypt Fiend VX , Muarki UVX , Ghoul , Ghoul VX , M'Karsa , Ancient Hydra , Vjaier UVX , Twins VX , The Twins , Zanatath , Zanatath VX , Ortakna , Quest: The Eastern Road, Crypt Fiend , Ghoul , Quest: The Meat Wagons, Hero Trial of Sylph, Parvin , Arturia , Parvin , Villard , Villard , Arkham , Arkham , Ripper , Arturia , Talus VX , Talus , Talus , Ripper
/// Possible class values: Phantom Stalker, Warrior, Chaotic Knight, Aeromancer, Shadow Shaman, Earthquaker, Arcanist, Chronowarper, Paladin, Ranger, Barbarian, Medicaster, Pyromancer, Venomancer, Cleric, Hydromancer, Shadowblade, Druid
/// </summary>
[UsedImplicitly]
public class FullTextSearchItemsHasCommand : BaseCommandModule, IHasCommandUsage
{
    private readonly IItemsRepository _repository;
    private readonly ISettings _settings;

    private readonly Regex _levelRegex = new(@"level[\:\s]*(\d+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    private readonly Regex _classRegex;

    public FullTextSearchItemsHasCommand(IItemsRepository repository, ISettings settings)
    {
        _repository = repository;
        _settings = settings;

        var classNames = Enum.GetNames(typeof(TkokClass)).Skip(1);
        var classNamesRegexGroup = string.Join("|", classNames.Select(n => Regex.Replace(n, @"([^^])([A-Z])", @"$1\s*?$2")));
        _classRegex = new Regex($@"class[\:\s]*({classNamesRegexGroup})", RegexOptions.Compiled | RegexOptions.IgnoreCase);
    }

    [Command("search")]
    [Aliases("s")]
    public async Task Search(CommandContext context)
    {
        var commandPrefixes = new[] { "!search ", "!s " };
        var message = context.Message.Content;
        if (commandPrefixes.All(commandPrefix => !message.StartsWith(commandPrefix)))
        {
            return;
        }

        if (!context.Channel.IsPrivate && context.Channel.Id != _settings.BotCommandsChannelId)
        {
            var botCommandChannel = context.Guild.GetChannel(_settings.BotCommandsChannelId);
            await context.RespondAsync($"Please use this command in {botCommandChannel.Mention} instead.");
            return;
        }

        //Parse input
        var msg = message.ToLower();
        var level = GetRegexResultAndReplace(_levelRegex, ref msg).ToNullableInt();
        var classString = GetRegexResultAndReplace(_classRegex, ref msg)?.Replace(" ", string.Empty);
        var tClass = TkokClass.None;

        if (!string.IsNullOrWhiteSpace(classString) &&
            Enum.TryParse(typeof(TkokClass), classString, true, out var c))
        {
            tClass = (TkokClass)c!;
        }

        var filter = commandPrefixes.Aggregate(msg, (current, cmdPrefix) => current.Replace(cmdPrefix, "")).Trim();

        //Fetch and display the items
        var items = (await _repository.FullTextSearchAsync(filter, level, tClass));
        if (!items.Any())
        {
            await context.RespondAsync(" <:monkaS:378522043970486275> There are no matching items.");
        }

        var messagePages = ItemFormatter.ToPages(items);
        if (messagePages.Count > 1)
        {
            var interactivity = context.Client.GetInteractivity();

            // Awaiting for completion of the interactivity session deadlocks *any* further
            // commands *on a host with single-core CPU*, but works  properly on multi-code CPU even
            // with InteractivityConfiguration.AckPaginationButtons set to `true`.
            //
            // It's a regression after upgrading to DSharpPlus 4.x from 3.x.
            //
            // Therefore, the task is not awaited below. It will be eventually completed
            // in background when interaction is over or on interactivity timeout.

            _ = interactivity.SendPaginatedMessageAsync(context.Channel, context.User, messagePages);
        }
        else
        {
            await context.RespondAsync(messagePages[0].Content);
        }
    }

    /// <summary>
    /// Function is used to parse the command message step-by-step
    /// </summary>
    /// <param name="regex"></param>
    /// <param name="input"></param>
    /// <returns></returns>
    private static string GetRegexResultAndReplace(Regex regex, ref string input)
    {
        var match = regex.Match(input);
        if (match.Success)
        {
            var result = match.Result("$1");
            input = regex.Replace(input, "");
            return result;
        }
        return null;
    }

    public CommandInfo GetUsage()
    {
        var info = new CommandInfo
        {
            Command = "!search <any filter> <class:ClassName> <level:XX>",
            Usage =
                "Search for items. All filters are optional, multiple filters can be combined. Possible values:\r\n" +
                "\t* **filter**: any item name, boss name, item type, slot, quality, does not have to be the full name\r\n" +
                "\t* **level**: number from 8 to 47\r\n" +
                "\t* **class**: class name (examples: Warrior, ChaoticKnight, Shadowblade, PhantomStalker, ShadowShaman, etc.)\r\n" +
                "\r\n" +
                "**Example usage**:\r\n" +
                "\t!search Ortakna VX\r\n" +
                "\t!search level:35 hat\r\n" +
                "\t!search hat level:35\r\n" +
                "\t!s class:PhantomStalker\r\n" +
                "\t!s class ranger shoulder\r\n" +
                "\t!s level 35 \r\n"
        };
        Console.WriteLine(info.Usage.Length);
        return info;
    }
}