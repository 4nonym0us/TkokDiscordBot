using TkokDiscordBot.Entities;

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
        //var reforgedItem = ObjectCopier.Clone(item);
        var reforgedItem = (Item)item.Clone();
        reforgedItem.ReforgeLevel = level;

        return reforgedItem;
    }
}