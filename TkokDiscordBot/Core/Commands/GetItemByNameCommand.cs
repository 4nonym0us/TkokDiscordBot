using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.Data.Abstractions;

namespace TkokDiscordBot.Core.Commands
{
    internal class GetItemByNameCommand : IBotCommand
    {
        private readonly IItemsRepository _repository;

        public GetItemByNameCommand(IItemsRepository repository)
        {
            _repository = repository;
        }

        public async Task<bool> Handle(DiscordClient sender, MessageCreateEventArgs eventArgs)
        {
            var message = eventArgs.Message.Content;
            if (!message.StartsWith("!"))
            {
                return false;
            }

            var filter = message.TrimStart('!').Trim();

            //Try to get exact match first , partial match if first lookup fails
            var item =
                await _repository.FirstOrDefault(i => string.Equals(i.Name, filter, StringComparison.CurrentCultureIgnoreCase)) ??
                (await _repository.Search(filter)).FirstOrDefault();

            if (item == null)
            {
                return false;
            }

            var builder = new DiscordEmbedBuilder();
            builder.WithColor(DiscordColor.Green);

            if (string.IsNullOrWhiteSpace(item.Description))
            {
                builder.WithTitle(item.Name);
            }
            else
            {
                builder.AddField(item.Name, $"```{item.Description}```");
            }

            var generalText = $"**Slot**: {item.Slot}\r\n" +
                              $"**Type**: {item.Type}\r\n" +
                              $"**Level**: {item.Level}\r\n" +
                              $"**Quality**: {item.Quality}\r\n";
            if (!string.IsNullOrWhiteSpace(item.ObtainableFrom))
            {
                generalText += $"**Obtained from**: {item.ObtainableFrom}\r\n";
            }
            builder.AddField("General", generalText, true);

            var statsText = string.Join("\r\n", item.Properties.Select(x => $"**{x.Key}**: {x.Value}"));
            if (!string.IsNullOrWhiteSpace(statsText))
            {
                builder.AddField("Stats", statsText, true);
            }

            if (!string.IsNullOrWhiteSpace(item.Special))
            {
                var special = Regex.Replace(item.Special, "(.+?):", "**$1**:");
                builder.AddField("Special effect", special);
            }

            if (!string.IsNullOrWhiteSpace(item.IconUrl))
            {
                builder.WithThumbnailUrl(item.IconUrl);
            }

            var embed = builder.Build();

            await eventArgs.Message.RespondAsync(string.Empty, false, embed);
            //if (eventArgs.Channel.Name == "bot-commands" || eventArgs.Channel.IsPrivate)
            //{
            //    await eventArgs.Message.RespondAsync(string.Empty, false, embed);
            //}
            //else
            //{
            //    var user = await sender.GetUserAsync(eventArgs.Author.Id);
            //    var dmChannel = await sender.CreateDmAsync(user);
            //    await dmChannel.SendMessageAsync(string.Empty, false, embed);
            //}

            return true;
        }

        public CommandInfo GetUsage()
        {
            var info = new CommandInfo();
            info.Command = "!<item name>";
            info.Usage = "Find item by name. Replace **<item name>** with a item name, does not have to be the full name\r\n";
            return info;
        }
    }
}