using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using JetBrains.Annotations;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.Data.Abstractions;

namespace TkokDiscordBot.Core.Commands;

/// <summary>
/// A custom command that allows users to get items by typing a name of the item after an exclamation mark.
/// Example command: `!Recondite Ring`.
/// </summary>
[UsedImplicitly]
public class GetItemByNameCommand : IBotCommand
{
    private readonly IItemsRepository _repository;
    private readonly ISettings _settings;

    public GetItemByNameCommand(IItemsRepository repository, ISettings settings)
    {
        _repository = repository;
        _settings = settings;
    }

    public async Task<bool> HandleAsync(DiscordClient client, MessageCreateEventArgs args)
    {
        var message = args.Message.Content;
        if (!message.StartsWith("!") || message.Length <= 3)
        {
            return false;
        }

        if (!args.Channel.IsPrivate &&
            args.Channel.Id != _settings.BotCommandsChannelId && args.Guild.Id == _settings.MainServerId)
        {
            var botCommandChannel = await client.GetChannelAsync(_settings.BotCommandsChannelId);
            await args.Message.RespondAsync($"Please use this command in {botCommandChannel.Mention} instead.");
            return true;
        }

        var filter = message.TrimStart('!').Trim();

        //Try to get exact match first , partial match if first lookup fails
        var item =
            _repository.Get(filter) ?? _repository.Search(filter).FirstOrDefault();

        if (item == null)
        {
            return false;
        }

        var builder = new DiscordEmbedBuilder();
        switch (item.Quality)
        {
            case "Enchanted":
                builder.WithColor(new DiscordColor(255, 255, 0));
                break;
            case "Epic":
                builder.WithColor(new DiscordColor(255, 102, 255));
                break;
            case "Relic":
                builder.WithColor(new DiscordColor(255, 204, 58));
                break;
            case "Artifact":
                builder.WithColor(new DiscordColor(205, 0, 0));
                break;
        }

        if (string.IsNullOrWhiteSpace(item.Description))
        {
            builder.WithTitle(item.ReforgedName);
        }
        else
        {
            builder.AddField(item.ReforgedName, $"```{item.Description}```");
        }

        //General group
        var generalText = $"**Slot**: {item.Slot}\r\n" +
                          $"**Type**: {item.Type}\r\n" +
                          $"**Level**: {item.Level}\r\n" +
                          $"**Quality**: {item.Quality}\r\n";

        if (!string.IsNullOrWhiteSpace(item.ObtainableFrom))
        {
            generalText += $"**Obtained from**: {item.ObtainableFrom}\r\n";
        }

        if (!string.IsNullOrWhiteSpace(item.ClassRestriction))
        {
            generalText += $"**Class restriction**: {item.ClassRestriction}\r\n";
        }

        builder.AddField("General", generalText, true);

        //Stats group
        var statsText = string.Join("\r\n", item.Properties.Select(x => $"**{x.Key}**: {x.Value:0.##}"));
        if (!string.IsNullOrWhiteSpace(statsText))
        {
            builder.AddField("Stats", statsText, true);
        }

        //Special effect group
        if (!string.IsNullOrWhiteSpace(item.Special))
        {
            var special = Regex.Replace(item.Special, "(.+?):", "**$1**:");
            builder.AddField("Special effect", special);
        }

        if (!string.IsNullOrWhiteSpace(item.IconUrl))
        {
            builder.WithThumbnail(item.IconUrl);
        }

        var embed = builder.Build();

        await args.Message.RespondAsync(string.Empty, embed);

        return true;
    }

    public CommandInfo GetUsage()
    {
        var info = new CommandInfo
        {
            Command = "`!<item name>`",
            Usage = "Find item by name. Replace **<item name>** with a item name, does not have to be the full name.\r\n",
            Order = -2
        };
        return info;
    }
}