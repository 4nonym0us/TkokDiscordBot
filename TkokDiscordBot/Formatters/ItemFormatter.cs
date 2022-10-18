using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using DSharpPlus.Interactivity;
using TkokDiscordBot.Entities;
using TkokDiscordBot.Extensions;

namespace TkokDiscordBot.Formatters;

public static class ItemFormatter
{
    public const int PageSize = 20;

    public static IList<Page> ToPages(IReadOnlyCollection<Item> items)
    {
        var pages = new List<Page>();
        var totalPages = (items.Count + PageSize - 1) / PageSize;

        for (var i = 0; i < totalPages; i++)
        {
            var pageItems = items.Skip(i * PageSize).Take(PageSize).ToList();
            var currentPageNumber = i + 1;

            pages.Add(new Page { Content = ToMarkdown(pageItems, currentPageNumber, totalPages) });
        }
        return pages;
    }

    public static string ToMarkdown(List<Item> items, int pageNumber, int totalPages)
    {
        var footer = new StringBuilder();

        if (totalPages > 1)
        {
            footer.Append($"Page **{pageNumber}** out of **{totalPages}**. ");

            if (pageNumber == 1)
            {
                footer.Append(" *Tip: Command sender can use emotes to see next pages.*");
            }
            else
            {
                var tipsPool = new[]
                {
                    "*Interactive pagination is automatically disabled in 5 minutes.*",
                    $"*You can use* `!{items[Random.Shared.Next(items.Count)].Name}` *to get more info about specific item.*",
                    $"*You can use* `!{items[Random.Shared.Next(items.Count)].Name} +{Random.Shared.Next(1, 4) * 5}` *to get reforged stats.*",
                    "*You can search items by type, slot, level, quality, boss. Use* `!help` *to find out how.*"
                };

                footer.Append($" *Tip:* {tipsPool.PickRandom()}");
            }
        }

        var output = new StringBuilder($"```\r\n{"Name",-33} | {"Type",12} | {"Slot",9} | {"Quality",10} | {"Level",3}\r\n{new string('=', 85)}\r\n");
        foreach (var item in items)
        {
            var line = $"{item.Name,-33} | {item.Type,12} | {item.Slot,9} | {item.Quality,10} | {item.Level,3}";
            if (output.Length + line.Length + footer.Length + 3 < 2000)
            {
                output.AppendLine(line);
            }
        }

        output.AppendLine($"```{footer}");

        return output.ToString();
    }
}