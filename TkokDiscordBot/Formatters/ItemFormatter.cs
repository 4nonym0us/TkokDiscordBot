using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using DSharpPlus.Interactivity;
using TkokDiscordBot.Entities;

namespace TkokDiscordBot.Formatters;

/// <summary>
/// Contains methods to help with properly formatting items in a search results table.
/// </summary>
public class DiscordPageGenerator
{
    public const int MaxPageContentLength = 2000;

    private static string[] SearchTooltips { get; } =
    {
        " *Tip: Command sender can use emotes to see next pages.*",
        " *Tip: Use* `!search-wizard` *(or* `!sw`*) command for better searching experience.*",
        " *Tip: Interactive pagination is automatically disabled in 5 minutes.*",
        " *Tip: You can search items by type, slot, level, quality, boss. Use* `!search-guide` *to find out how.*",
        " *Tip: Use* `!explore` *to find out more about the items and their properties.*",
        " *Tip: Use* `!<item name>` *to get more info about specific item.*",
        " *Tip: Use* `!<item name> +15` *to get stats of lvl 15 reforged item.*",
    };

    public IReadOnlyCollection<Page> ToPages(IReadOnlyCollection<Item> items, string header = null)
    {
        var colWidthSettings = new int[6];
        foreach (var item in items)
        {
            if (colWidthSettings[0] < item.Name.Length) { colWidthSettings[0] = item.Name.Length; }
            if (colWidthSettings[1] < item.Type.Length) { colWidthSettings[1] = item.Type.Length; }
            if (colWidthSettings[2] < item.Slot.Length) { colWidthSettings[2] = item.Slot.Length; }
            if (colWidthSettings[3] < item.Quality.Length) { colWidthSettings[3] = item.Quality.Length; }
            if (colWidthSettings[4] < item.Level.ToString().Length) { colWidthSettings[4] = item.Level.ToString().Length; }
            if (colWidthSettings[5] < item.NormalizedObtainableFrom.Length) { colWidthSettings[5] = item.NormalizedObtainableFrom.Length; }
        }

        return GeneratePagedOutput(items, colWidthSettings, header).ToList();
    }

    public IEnumerable<Page> GeneratePagedOutput(IReadOnlyCollection<Item> items, int[] colWidthSettings, string header = null)
    {
        const string verticalLine = " │ ";
        const char horizontalLine = '─';
        const string lineCrossing = "─┼─";
        var extraLength = "```".Length * 2;
        var headerLength = header?.Length ?? 0;

        var tableHeader = new StringBuilder()
            // 1st line
            .Append("Name".PadRight(colWidthSettings[0])).Append(verticalLine)
            .Append("Type".PadLeft(colWidthSettings[1])).Append(verticalLine)
            .Append("Slot".PadLeft(colWidthSettings[2])).Append(verticalLine)
            .Append("Quality".PadLeft(colWidthSettings[3])).Append(verticalLine)
            .Append("Lv".PadLeft(colWidthSettings[4])).Append(verticalLine)
            .Append("Source".PadLeft(colWidthSettings[5])).Append("\r\n")
            // 2nd line
            .Append(new string(horizontalLine, colWidthSettings[0])).Append(lineCrossing)
            .Append(new string(horizontalLine, colWidthSettings[1])).Append(lineCrossing)
            .Append(new string(horizontalLine, colWidthSettings[2])).Append(lineCrossing)
            .Append(new string(horizontalLine, colWidthSettings[3])).Append(lineCrossing)
            .Append(new string(horizontalLine, colWidthSettings[4])).Append(lineCrossing)
            .Append(new string(horizontalLine, colWidthSettings[5])).Append("\r\n")
            .ToString();

        var pages = new Collection<Page>();
        var tableContentBuilder = new StringBuilder(tableHeader);

        foreach (var item in items)
        {
            var pageNum = pages.Count + 1;
            var footerLength = GetFooterTemplate(pageNum).Length;

            var tableRowBuilder = new StringBuilder()
                .Append(item.Name.PadRight(colWidthSettings[0])).Append(verticalLine)
                .Append(item.Type.PadLeft(colWidthSettings[1])).Append(verticalLine)
                .Append(item.Slot.PadLeft(colWidthSettings[2])).Append(verticalLine)
                .Append(item.Quality.PadLeft(colWidthSettings[3])).Append(verticalLine)
                .Append(item.Level.ToString().PadLeft(colWidthSettings[4])).Append(verticalLine)
                .Append(item.NormalizedObtainableFrom.PadLeft(colWidthSettings[5]));

            // Check whether the page has spare space left
            if (headerLength + tableContentBuilder.Length + footerLength + tableRowBuilder.Length + extraLength < MaxPageContentLength)
            {
                // Append to current page
                tableContentBuilder.AppendLine(tableRowBuilder.ToString());
            }
            else
            {
                // Append to the next page
                pages.Add(new Page($"{header}```{tableContentBuilder}```"));
                tableContentBuilder = new StringBuilder(tableHeader).AppendLine(tableRowBuilder.ToString());
            }
        }

        // Preserve current page if there are any items in the buffer
        if (tableContentBuilder.Length > tableHeader.Length)
        {
            pages.Add(new Page($"{header}```{tableContentBuilder}```"));
        }

        // Append footers with correct page numbers
        for (var i = 0; i < pages.Count; i++)
        {
            var pageNum = i + 1;

            var footer = GetFooterTemplate(pageNum);
            pages[i].Content += string.Format(footer, pageNum, pages.Count);
        }

        return pages;
    }

    private static string GetFooterTemplate(int pageNumber)
    {
        var tooltip = SearchTooltips[(pageNumber - 1) % SearchTooltips.Length];
        return $"Page **{{0}}** out of **{{1}}**. {tooltip}";
    }
}