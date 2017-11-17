using System;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;

namespace TkokDiscordBot.Core.CommandsNext
{
    internal class PollCommand : ICommandNext
    {
        [Command("poll")]
        public async Task Poll(CommandContext ctx, string title, TimeSpan duration, params DiscordEmoji[] options)
        {
            // first retrieve the interactivity module from the client
            var interactivity = ctx.Client.GetInteractivityModule();
            var pollOptions = options.Select(xe => xe.ToString());

            // then let's present the poll
            var embed = new DiscordEmbedBuilder
            {
                Title = title,
                Description = string.Join(" ", pollOptions)
            };
            var msg = await ctx.RespondAsync(embed: embed);

            // add the options as reactions
            foreach (DiscordEmoji emoji in options)
                await msg.CreateReactionAsync(emoji);

            // collect and filter responses
            var pollResult = await interactivity.CollectReactionsAsync(msg, duration);
            var results = pollResult.Reactions.Where(xkvp => options.Contains(xkvp.Key))
                .Select(xkvp => $"{xkvp.Key}: {xkvp.Value}");

            // and finally post the results
            await ctx.RespondAsync("Poll results:\n" + string.Join("\n", results));
        }

        public CommandInfo GetUsage()
        {
            return null;
        }
    }
}
