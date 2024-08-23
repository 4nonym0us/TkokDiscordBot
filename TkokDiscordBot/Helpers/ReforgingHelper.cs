using System;
using System.Linq;
using TkokDiscordBot.Entities;

namespace TkokDiscordBot.Helpers;

internal static class ReforgingHelper
{
    private const double ReforgingMultiplier = 0.03334;
    private static readonly string[] PropertiesToTruncateOnReforge =
    {
        "Power",
        "Agility",
        "Energy",
        "Damage",
        "Armor",
        "Move speed",
        "Life",
        "Mana"
    };

    internal static void ReforgeProperties(Item item, short reforgeLevel)
    {
        foreach (var key in item.Properties.Keys.ToList())
        {
            var shouldBeTruncated = PropertiesToTruncateOnReforge.Any(x => x.Equals(key, StringComparison.OrdinalIgnoreCase));
            var reforgedValue = Math.Round(item.Properties[key] * (1 + reforgeLevel * ReforgingMultiplier), 2);

            item.Properties[key] = shouldBeTruncated ? Math.Truncate(reforgedValue) : reforgedValue;
        }
    }
}