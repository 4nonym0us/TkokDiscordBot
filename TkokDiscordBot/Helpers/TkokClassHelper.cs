using System;
using System.Collections.Generic;
using System.Linq;
using TkokDiscordBot.Entities;
using TkokDiscordBot.Enums;

namespace TkokDiscordBot.Helpers;

public static class TkokClassHelper
{
    private static readonly Dictionary<TkokClass, string[]> EquipableItemsDict;

    static TkokClassHelper()
    {
        EquipableItemsDict = new Dictionary<TkokClass, string[]>
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
    }

    public static Func<Item, bool> GetPredicateForItemLookup(TkokClass @class)
    {
        if (@class == TkokClass.None)
            return null;

        var itemTypes = EquipableItemsDict[@class];
        Func<Item, bool> classFilterPredicate;

        if (@class == TkokClass.Warrior)
            classFilterPredicate = item => itemTypes.Contains(item.Type) || item.Quality == "Epic" && item.Type == "Leather";
        else
            classFilterPredicate = item => itemTypes.Contains(item.Type);

        return classFilterPredicate;
    }
}