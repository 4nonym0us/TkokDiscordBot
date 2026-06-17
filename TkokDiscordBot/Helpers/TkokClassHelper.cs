using System;
using System.Collections.Generic;
using System.Linq;
using TkokDiscordBot.Entities;
using TkokDiscordBot.Enums;
using TkokDiscordBot.Extensions;

namespace TkokDiscordBot.Helpers;

public static class TkokClassHelper
{
    private static readonly Dictionary<TkokClass, string[]> ItemTypesByClassDict;
    private static readonly Dictionary<TkokClass, string> HumanFriendlyNamesDict;

    static TkokClassHelper()
    {
        ItemTypesByClassDict = new Dictionary<TkokClass, string[]>
        {
            { TkokClass.Arcanist, ["Mithril", "Cloth", "Staff", "Book", "Wand", "Orb"] },
            { TkokClass.Hydromancer, ["Mithril", "Cloth", "Staff", "Book", "Wand", "Orb"] },
            { TkokClass.Pyromancer, ["Mithril", "Cloth", "Staff", "Book", "Wand", "Orb"] },
            { TkokClass.Aeromancer, ["Mithril", "Cloth", "Staff", "Book", "Wand", "Orb"] },
            { TkokClass.Cleric, ["Mithril", "Cloth", "Staff", "Book", "Wand", "Orb"] },
            { TkokClass.Warrior, ["Mithril", "Mail", "Sword", "Axe", "Mace", "Shield", "Dual Dagger", "Dual Axe"] },
            { TkokClass.ChaoticKnight, ["Mithril", "Mail", "Sword", "Axe", "Shield", "Dual Axe"] },
            { TkokClass.Shadowblade, ["Mithril", "Leather", "Dagger", "Sword", "Dual Dagger"] },
            { TkokClass.Medicaster, ["Mithril", "Leather", "Cloth", "Staff", "Axe", "Mace", "Idol", "Book"] },
            { TkokClass.Venomancer, ["Mithril", "Leather", "Dagger", "Dual Dagger", "Wand", "Orb"] },
            { TkokClass.PhantomStalker, ["Mithril", "Leather", "Dagger", "Dual Dagger"] },
            { TkokClass.Chronowarper, ["Mithril", "Cloth", "Mail", "Sword", "Axe", "Mace", "Dual Dagger", "Dual Axe", "Wand", "Orb"] },
            { TkokClass.Barbarian, ["Mithril", "Mail", "Leather", "Axe", "Dual Axe"] },
            { TkokClass.Paladin, ["Mithril", "Mail", "Mace", "Book"] },
            { TkokClass.Ranger, ["Mithril", "Leather", "Bow", "Quiver", "Dual Dagger"] },
            { TkokClass.Druid, ["Mithril", "Leather", "Staff", "Idol"] },
            { TkokClass.Earthquaker, ["Mithril", "Mail", "Axe", "Dual Axe", "Idol"] },
            { TkokClass.ShadowShaman, ["Mithril", "Cloth", "Staff", "Idol", "Wand", "Orb"] }
        };

        HumanFriendlyNamesDict = Enum.GetValues<TkokClass>().ToDictionary(c => c, c => c.ToString().ToTitleCase());
    }

    /// <summary>
    /// Returns a human-friendly string representation of specified class name.
    /// Example: `TkokClass.ShadowShaman` gets converted to `Shadow Shaman`.
    /// </summary>
    /// <param name="tkokClass"></param>
    /// <returns></returns>
    public static string GetClassName(TkokClass tkokClass) => HumanFriendlyNamesDict[tkokClass];

    /// <summary>
    /// Attempts to parse a string representation of class name to <see cref="TkokClass"/>.
    /// </summary>
    /// <param name="className"></param>
    /// <param name="tkokClass"></param>
    /// <returns></returns>
    public static bool TryParse(string className, out TkokClass tkokClass)
    {
        if (Enum.TryParse(typeof(TkokClass), className, true, out var c))
        {
            tkokClass = (TkokClass)c!;
            return true;
        }

        tkokClass = default;
        return false;
    }

    /// <summary>
    /// Returns list of classes that can equip specific item.
    /// Note: does not take accessories into account because they available for everyone.
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    public static IEnumerable<TkokClass> GetUsersOfItem(Item item)
    {
        if (!item.ClassRestriction.IsNullOrEmpty() && TryParse(item.ClassRestriction, out var tkokClass))
        {
            yield return tkokClass;
        }
        else
        {
            foreach (var @class in Enum.GetValues<TkokClass>())
            {
                if (GetPredicateForItemLookup(@class)(item))
                {
                    yield return @class;
                }
            }
        }
    }

    /// <summary>
    /// Returns a predicate to test whether an item can be equipped by specific class.
    /// Note: does not take accessories into account because they available for everyone.
    /// </summary>
    /// <param name="tkokClass"></param>
    /// <returns></returns>
    public static Func<Item, bool> GetPredicateForItemLookup(TkokClass tkokClass)
    {
        var itemTypes = ItemTypesByClassDict[tkokClass];

        return tkokClass == TkokClass.Warrior
            ? item => itemTypes.Contains(item.Type, true) || item.Quality == "Epic" && item.Type == "Leather"
            : item => itemTypes.Contains(item.Type, true);
    }
}