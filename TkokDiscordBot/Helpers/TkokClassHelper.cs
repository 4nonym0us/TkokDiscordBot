using System;
using System.Collections.Generic;
using System.Linq;
using TkokDiscordBot.Entities;
using TkokDiscordBot.Enums;

namespace TkokDiscordBot.Helpers
{
    public static class TkokClassHelper
    {
        private static readonly Dictionary<TkokClass, string[]> EquipableItemsDict;

        static TkokClassHelper()
        {
            EquipableItemsDict = new Dictionary<TkokClass, string[]>
            {
                {TkokClass.Arcanist, new[] {"Cloth", "Staff, Book", "Wand", "Orb"}},
                {TkokClass.Hydromancer, new[] {"Cloth", "Staff, Book", "Wand", "Orb"}},
                {TkokClass.Pyromancer, new[] {"Cloth", "Staff, Book", "Wand", "Orb"}},
                {TkokClass.Aeromancer, new[] {"Cloth", "Staff, Book", "Wand", "Orb"}},
                {TkokClass.Cleric, new[] {"Cloth", "Staff, Book", "Wand", "Orb"}},
                {TkokClass.Warrior, new[] {"Chain","Sword","Axe","Mace, Shield","Dual Dagger","Dual Axe"}},
                {TkokClass.ChaoticKnight, new[] {"Chain","Sword","Axe, Shield","Dual Axe"}},
                {TkokClass.Shadowblade, new[] {"Leather","Dagger","Sword, Dual Dagger"}},
                {TkokClass.Medicaster, new[] {"Leather or Cloth","Staff","Axe","Mace, Idol","Book"}},
                {TkokClass.Venomancer, new[] {"Leather","Dagger, Dual Dagger","Wand","Orb"}},
                {TkokClass.PhantomStalker, new[] {"Leather","Dagger, Dual Dagger"}},
                {TkokClass.Chronowarper, new[] {"Cloth or Chain","Sword","Axe","Mace, Dual Dagger","Dual Axe","Wand","Orb"}},
                {TkokClass.Barbarian, new[] {"Chain or Leather","Axe, Dual Axe"}},
                {TkokClass.Paladin, new[] {"Chain","Mace, Book"}},
                {TkokClass.Ranger, new[] {"Leather","Bow, Quiver","Dual Dagger"}},
                {TkokClass.Druid, new[] {"Leather","Staff, Idol"}},
                {TkokClass.Earthquaker, new[] {"Chain","Axe, Dual Axe","Idol"}},
                {TkokClass.ShadowShaman, new[] {"Cloth","Staff, Idol","Wand","Orb"}}
            };
        }

        public static Func<Item, bool> GetPredicateForItemLookup(TkokClass @class)
        {
            if (@class == TkokClass.None)
            {
                return null;
            }

            var itemTypes = EquipableItemsDict[@class];
            Func<Item, bool> classFilterPredicate;

            if (@class == TkokClass.Warrior)
            {
                classFilterPredicate = item =>
                    itemTypes.Contains(item.Type) || item.Quality == "Epic" && item.Type == "Leather";
            }
            else
            {
                classFilterPredicate = item => itemTypes.Contains(item.Type);
            }

            return classFilterPredicate;
        }
    }
}