using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Entities;
using TkokDiscordBot.Enums;
using TkokDiscordBot.Extensions;
using TkokDiscordBot.Helpers;

namespace TkokDiscordBot.Analysis;

public class ItemAnalysisService : IItemAnalysisService
{
    public List<string> AvailableClasses { get; } = [];

    public List<string> ItemSlots { get; } = [];

    public List<string> ItemTypes { get; } = [];

    public List<string> ItemSources { get; } = [];

    public List<string> QualityLevels { get; } = [];

    public List<string> AvailableBosses { get; } = [];

    public ItemAnalysisService(IItemsStore itemsStore)
    {
        RecalculateStats(itemsStore.GetAll());

        itemsStore.ItemsReloaded += ItemsStoreOnItemsReloaded;
    }

    private void ItemsStoreOnItemsReloaded(object sender, IReadOnlyCollection<Item> items)
    {
        RecalculateStats(items);
    }

    protected void RecalculateStats(IReadOnlyCollection<Item> items)
    {
        AvailableClasses.Clear();
        ItemSlots.Clear();
        ItemTypes.Clear();
        ItemSources.Clear();
        QualityLevels.Clear();

        AvailableClasses.AddRange(Enum.GetValues<TkokClass>().Select(TkokClassHelper.GetClassName));

        foreach (var item in items)
        {
            if (!item.Slot.IsNullOrEmpty() && !ItemSlots.Contains(item.Slot))
                ItemSlots.Add(item.Slot);

            if (!item.Type.IsNullOrEmpty() && !ItemTypes.Contains(item.Type))
                ItemTypes.Add(item.Type);

            if (!item.Quality.IsNullOrEmpty() && !QualityLevels.Contains(item.Quality))
                QualityLevels.Add(item.Quality);

            var itemSource = Regex.Replace(item.ObtainableFrom, @"\s*?(\[.*?\]|^Any .*)\s*?", string.Empty);
            if (item.ObtainableFrom.Contains("Empowered")) { itemSource += " Empowered"; }
            if (item.ObtainableFrom.Contains("Champion")) { itemSource += " Champion"; }

            if (!itemSource.IsNullOrEmpty() && !ItemSources.Any(b => string.Equals(b, itemSource, StringComparison.InvariantCultureIgnoreCase)))
                ItemSources.Add(itemSource);
        }

        if (!AvailableBosses.Any())
        {
            AvailableBosses.AddRange([
                "Broodmother", "Narith", "Sand Golem", "Naztar", "Karrix", "Avnos", "Karnos", "Karavnos", "Muarki",
                "Vjaier", "Crueltis", "Tal'Navi", "M'Karsa", "Hydra", "Ortakna", "Crypt Fiend", "Ghoul",
                "Ancient Hydra", "Twins", "Zanatath", "Parvin", "Arturia", "Villard", "Arkham", "Ripper", "Talus"
            ]);
        }
    }
}