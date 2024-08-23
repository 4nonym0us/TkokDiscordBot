using System.Linq;
using Lucene.Net.Documents;
using Lucene.Net.Documents.Extensions;
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
        var normalizedItemSource = item.NormalizedObtainableFrom;
        if (item.ObtainableFrom.Contains("Empowered")) { normalizedItemSource += " Empowered"; }
        if (item.ObtainableFrom.Contains("Champion")) { normalizedItemSource += " Champion"; }

        var doc = new Document
        {
            new Int32Field("id", item.Id, Field.Store.YES),
            new TextField("name", item.Name, Field.Store.YES),
            new TextField("slot", item.Slot, Field.Store.YES),
            new TextField("type", item.Type, Field.Store.YES),
            new TextField("quality", item.Quality, Field.Store.YES),
            new TextField("source", normalizedItemSource, Field.Store.YES),
            new TextField("level", item.Level.ToString(), Field.Store.YES),
            new TextField("description", item.Description, Field.Store.YES),
            new TextField("special", item.Special ?? string.Empty, Field.Store.YES)
        };

        var usersOfAnItem = TkokClassHelper.GetUsersOfItem(item).Select(TkokClassHelper.GetClassName);

        foreach (var className in usersOfAnItem)
        {
            doc.AddTextField("class", string.Join(", ", className), Field.Store.YES);
        }

        return doc;
    }
}