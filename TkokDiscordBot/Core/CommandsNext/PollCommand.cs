using System;
using System.Linq;
using System.Threading.Tasks;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using JetBrains.Annotations;

namespace TkokDiscordBot.Core.CommandsNext;

[Hidden]
[UsedImplicitly]
public class PollHasCommand : BaseCommandModule
{
    [Command("poll")]
    public async Task Poll(CommandContext ctx, string title, TimeSpan duration, params DiscordEmoji[] options)
    {
        // first retrieve the interactivity module from the client
        var interactivity = ctx.Client.GetInteractivity();
        var pollOptions = options.Select(xe => xe.ToString());

        // then let's present the poll
        var embed = new DiscordEmbedBuilder
        {
            Title = title,
            Description = string.Join(" ", pollOptions)
        };
        var msg = await ctx.RespondAsync(embed: embed);

        // add the options as reactions
        foreach (var emoji in options)
            await msg.CreateReactionAsync(emoji);

        // collect and filter responses
        var pollResult = await interactivity.CollectReactionsAsync(msg, duration);
        var results = pollResult.Where(xkvp => options.Contains(xkvp.Emoji))
            .Select(xkvp => $"{xkvp.Emoji}: {xkvp.Total}");

        // and finally post the results
        await ctx.RespondAsync("Poll results:\n" + string.Join("\n", results));
    }
}