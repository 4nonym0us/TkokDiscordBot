using System.Linq;
using System.Text.RegularExpressions;
using Lucene.Net.Documents;
using TkokDiscordBot.Entities;
using TkokDiscordBot.Helpers;

namespace TkokDiscordBot.Extensions;

public static class ItemExtensions
{
    /// <summary>
    /// Returned reforged copy of an item. Does not modify item in place.
    /// </summary>
    /// <param name="item"></param>
    /// <param name="level"></param>
    /// <returns></returns>
    public static Item Reforged(this Item item, short level)
    {
        var reforgedItem = (Item)item.Clone();
        reforgedItem.ReforgeLevel = level;

        return reforgedItem;
    }

    /// <summary>
    /// Convert an item to a Lucene document, which can be used in an Index.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static Document ToDocument(this Item item)
    {
        var usersOfAnItem = TkokClassHelper.GetUsersOfItem(item).Select(TkokClassHelper.GetClassName);

        var normalizedItemSource = Regex.Replace(item.ObtainableFrom, @"\s*?\[.*?\]\s*?", string.Empty);
        if (item.ObtainableFrom.Contains("Empowered")) { normalizedItemSource += " Empowered"; }
        if (item.ObtainableFrom.Contains("Champion")) { normalizedItemSource += " Champion"; }

        return new Document
        {
            new Int32Field("id", item.Id, Field.Store.YES),
            new TextField("name", item.Name, Field.Store.YES),
            new TextField("slot", item.Slot, Field.Store.YES),
            new TextField("type", item.Type, Field.Store.YES),
            new TextField("quality", item.Quality, Field.Store.YES),
            new TextField("obtainableFrom", normalizedItemSource, Field.Store.YES),
            new TextField("level", item.Level.ToString(), Field.Store.YES),
            new TextField("supportedClasses", string.Join(", ", usersOfAnItem), Field.Store.YES),
            new TextField("description", item.Description, Field.Store.YES),
            new TextField("special", item.Special ?? string.Empty, Field.Store.YES)
        };
    }
}