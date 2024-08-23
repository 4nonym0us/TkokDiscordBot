using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Attributes;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Entities;
using TkokDiscordBot.Enums;
using TkokDiscordBot.Extensions;

namespace TkokDiscordBot.Core.Commands
{
    [Priority(CommandPriority.Low)]
    internal class FullTextSearchItemsCommand : IBotCommand
    {
        private readonly IItemsRepository _repository;

        public FullTextSearchItemsCommand(IItemsRepository repository)
        {
            _repository = repository;
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

        public async Task<bool> Handle(DiscordClient sender, MessageCreateEventArgs eventArgs)
        {
            var commandPrefixes = new []{ "!search " , "!s "};
            var message = eventArgs.Message.Content;
            if (commandPrefixes.All(commandPrefix => !message.StartsWith(commandPrefix)))
            {
                return false;
            }

            //Parse input
            var msg = message.ToLower();
            var levelRegex = new Regex(@"level[\:\s]*(\d+)");
            var skipRegex = new Regex(@"skip[\:\s]*(\d+)");
            var classRegex = new Regex(@"class[\:\s]*([A-Za-z]+)");
            var level = GetRegexResultAndReplace(levelRegex, ref msg).ToNullableInt();
            var skip = GetRegexResultAndReplace(skipRegex, ref msg).ToNullableInt();
            var classString = GetRegexResultAndReplace(classRegex, ref msg);
            var @class = TkokClass.None;
            if (!string.IsNullOrWhiteSpace(classString))
            {
                @class = (TkokClass) Enum.Parse(typeof(TkokClass), classString, true);
            }
            var filter = msg.Replace("!search ", "").Replace("!s ", "").Trim();

            //Fetch and display the items
            var items = (await _repository.FullTextSearch(filter, level, @class)).ToList();
            var totalItems = items.Count;
            items = items.Skip(skip ?? 0).Take(20).ToList();
            if (!items.Any())
            {
                await eventArgs.Message.RespondAsync(" <:monkaS:378522043970486275> There are no matching items.");
                return true;
            }

            var displayedItems = 0;
            var footer = $"Displayed items: **%displayed_items%** ";
            if (totalItems > items.Count)
            {
                footer += $"out of **{totalItems}** ";
                if (!skip.HasValue)
                {
                    footer += $"\r\n*Tip: Use 'skip:%number%' parameter to see the rest of items.*";
                }
                else
                {
                    footer += $"*(skipped first **{skip}** items)*";
                }
            }

            var output = $"```\r\n{"Name",-33} | {"Type",12} | {"Slot",9} | {"Quality",10} | {"Level",3}\r\n" +
                           new string('=', 85) + "\r\n";
            foreach (var item in items)
            {
                var line = $"{item.Name,-33} | {item.Type,12} | {item.Slot,9} | {item.Quality,10} | {item.Level,3}\r\n";
                if (output.Length + line.Length + footer.Length + 3 < 2000)
                {
                    displayedItems++;
                    output += line;
                }
            }
            output += "```\r\n" + footer.Replace("%displayed_items%", displayedItems.ToString());

            if (eventArgs.Channel.Name == "bot-commands" || eventArgs.Channel.IsPrivate)
            {
                await eventArgs.Message.RespondAsync(output);
            }
            else
            {
                var user = await sender.GetUserAsync(eventArgs.Author.Id);
                var dmChannel = await sender.CreateDmAsync(user);
                await dmChannel.SendMessageAsync(output);

                await eventArgs.Message.RespondAsync($"{user.Mention} Non-#bot-commands channel detected. Current command output is sent to your PM.");
                return true;
            }

            return true;
        }

        public CommandInfo GetUsage()
        {
            var info = new CommandInfo();
            info.Command = "!search <any filter> <class:ClassName> <level:XX> <skip:YY>";
            info.Usage =
                "Search for items. All filters are optional, multiple filters can be combined. Possible values:\r\n" +
                "\t* **filter**: any item name, boss name, item type, slot, quality, does not have to be the full name\r\n" +
                "\t* **level**: number from 8 to 47\r\n" +
                "\t* **class**: class name (examples: Warrior, ChaoticKnight, Shadowblade, PhantomStalker, ShadowShaman, etc.)\r\n" +
                "\t* **skip**: number from 0 to any number (used to paginate results as Discord shows only 20 items at once)\r\n\r\n" +
                "**Example usage**:\r\n" +
                "\t!search Ortakna VX\r\n" +
                "\t!search level:35 hat\r\n" +
                "\t!search hat level:35\r\n" +
                "\t!s class:PhantomStalker skip:20\r\n" +
                "\t!s class ranger shoulder\r\n" +
                "\t!s level 35 \r\n";
            return info;
        }
    }
}
