using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;
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
            { TkokClass.Arcanist, new[] { "Mithril", "Cloth", "Staff", "Book", "Wand", "Orb" } },
            { TkokClass.Hydromancer, new[] { "Mithril", "Cloth", "Staff", "Book", "Wand", "Orb" } },
            { TkokClass.Pyromancer, new[] { "Mithril", "Cloth", "Staff", "Book", "Wand", "Orb" } },
            { TkokClass.Aeromancer, new[] { "Mithril", "Cloth", "Staff", "Book", "Wand", "Orb" } },
            { TkokClass.Cleric, new[] { "Mithril", "Cloth", "Staff", "Book", "Wand", "Orb" } },
            { TkokClass.Warrior, new[] { "Mithril", "Mail", "Sword", "Axe", "Mace", "Shield", "Dual Dagger", "Dual Axe" } },
            { TkokClass.ChaoticKnight, new[] { "Mithril", "Mail", "Sword", "Axe", "Shield", "Dual Axe" } },
            { TkokClass.Shadowblade, new[] { "Mithril", "Leather", "Dagger", "Sword", "Dual Dagger" } },
            { TkokClass.Medicaster, new[] { "Mithril", "Leather", "Cloth", "Staff", "Axe", "Mace", "Idol", "Book" } },
            { TkokClass.Venomancer, new[] { "Mithril", "Leather", "Dagger", "Dual Dagger", "Wand", "Orb" } },
            { TkokClass.PhantomStalker, new[] { "Mithril", "Leather", "Dagger", "Dual Dagger" } },
            { TkokClass.Chronowarper, new[] { "Mithril", "Cloth", "Mail", "Sword", "Axe", "Mace", "Dual Dagger", "Dual Axe", "Wand", "Orb" } },
            { TkokClass.Barbarian, new[] { "Mithril", "Mail", "Leather", "Axe", "Dual Axe" } },
            { TkokClass.Paladin, new[] { "Mithril", "Mail", "Mace", "Book" } },
            { TkokClass.Ranger, new[] { "Mithril", "Leather", "Bow", "Quiver", "Dual Dagger" } },
            { TkokClass.Druid, new[] { "Mithril", "Leather", "Staff", "Idol" } },
            { TkokClass.Earthquaker, new[] { "Mithril", "Mail", "Axe", "Dual Axe", "Idol" } },
            { TkokClass.ShadowShaman, new[] { "Mithril", "Cloth", "Staff", "Idol", "Wand", "Orb" } }
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
            ? item => itemTypes.Contains(item.Type) || item.Quality == "Epic" && item.Type == "Leather"
            : item => itemTypes.Contains(item.Type);
    }
}