using System;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Interactivity;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Enums;
using TkokDiscordBot.Extensions;
using TkokDiscordBot.Formatters;

namespace TkokDiscordBot.Core.CommandsNext
{
    internal class FullTextSearchItemsCommand : ICommandNext
    {
        private readonly IItemsRepository _repository;
        private readonly ISettings _settings;

        public FullTextSearchItemsCommand(IItemsRepository repository, ISettings settings)
        {
            _repository = repository;
            _settings = settings;
        }

        /// <summary>
        /// Function is used to parse the command message step-by-step
        /// </summary>
        /// <param name="regex"></param>
        /// <param name="input"></param>
        /// <returns></returns>
        private string GetRegexResultAndReplace(Regex regex, ref string input)
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

            if (context.Channel.Id == _settings.MediaChannelId)
            {
                var botCommandChannel = context.Guild.GetChannel(_settings.BotCommandsChannelId);
                await context.RespondAsync($"Please use this command in {botCommandChannel.Mention} instead.");
                return;
            }

            //Parse input
            var msg = message.ToLower();
            var levelRegex = new Regex(@"level[\:\s]*(\d+)");
            var classRegex = new Regex(@"class[\:\s]*([A-Za-z]+)");
            var level = GetRegexResultAndReplace(levelRegex, ref msg).ToNullableInt();
            var classString = GetRegexResultAndReplace(classRegex, ref msg);
            var @class = TkokClass.None;
            if (!string.IsNullOrWhiteSpace(classString))
            {
                @class = (TkokClass)Enum.Parse(typeof(TkokClass), classString, true);
            }

            var filter = commandPrefixes.Aggregate(msg, (current, cmdPrefix) => current.Replace(cmdPrefix, "")).Trim();

            //Fetch and display the items
            var items = (await _repository.FullTextSearch(filter, level, @class)).ToList();
            if (!items.Any())
            {
                await context.RespondAsync(" <:monkaS:378522043970486275> There are no matching items.");
            }

            var messagePages = ItemFormatter.ToPages(items);
            if (messagePages.Count() > 1)
            {
                var interactivity = context.Client.GetInteractivityModule();
                await interactivity.SendPaginatedMessage(context.Channel, context.User, messagePages);
            }
            else
            {
                await context.RespondAsync(messagePages[0].Content);
            }
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
                    "**Example usage**:\r\n" +
                    "\t!search Ortakna VX\r\n" +
                    "\t!search level:35 hat\r\n" +
                    "\t!search hat level:35\r\n" +
                    "\t!s class:PhantomStalker\r\n" +
                    "\t!s class ranger shoulder\r\n" +
                    "\t!s level 35 \r\n"
            };
            return info;
        }
    }
}
