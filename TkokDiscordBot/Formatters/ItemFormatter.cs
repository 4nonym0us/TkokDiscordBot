using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Interactivity;
using TkokDiscordBot.Entities;

namespace TkokDiscordBot.Formatters
{
    public static class ItemFormatter
    {
        public static IList<Page> ToPages(List<Item> items, int? skip = null)
        {
            var pages = new List<Page>();
            for (var i = 0; i < items.Count; i += 20)
            {
                pages.Add(new Page { Content = ToMarkdown(items, i) });
            }
            return pages;
        }

        public static string ToMarkdown(List<Item> items, int? skip = null)
        {
            var totalItems = items.Count;
            items = items.Skip(skip ?? 0).Take(20).ToList();

            var footer = string.Empty;
            var currentPage = skip / 20 + 1;
            var totalPages = totalItems / 20 + 1;

            if (totalItems > items.Count)
            {
                footer += $"Page **{currentPage}** out of **{totalPages}**. ";
                if (!skip.HasValue || skip.Value == 0 && totalPages > 1)
                {
                    footer += " *Tip: Command sender can use emotes to see next pages.*";
                }
            }

            var output = $"```\r\n{"Name",-33} | {"Type",12} | {"Slot",9} | {"Quality",10} | {"Level",3}\r\n" +
                         new string('=', 85) + "\r\n";
            foreach (var item in items)
            {
                var line = $"{item.Name,-33} | {item.Type,12} | {item.Slot,9} | {item.Quality,10} | {item.Level,3}\r\n";
                if (output.Length + line.Length + footer.Length + 3 < 2000)
                {
                    output += line;
                }
            }
            output += "```\r\n" + footer;

            return output;
        }
    }
}
