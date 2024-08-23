using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Entities;
using TkokDiscordBot.Extensions;

namespace TkokDiscordBot.Data;

public class ItemsRepository : IItemsRepository
{
    private readonly IItemsStore _itemsStore;
    private readonly Regex _reforgedItemRegex = new(@"^(.*?)\s*?\+([1-9]|1[0-5])$", RegexOptions.Compiled);

    public ItemsRepository(IItemsStore itemsStore)
    {
        _itemsStore = itemsStore;
    }

    public IReadOnlyCollection<Item> GetAll()
    {
        return _itemsStore.GetAll();
    }

    public Item Get(string name)
    {
        var query = _itemsStore.GetAll();
        var reforgedItemMatch = _reforgedItemRegex.Match(name);

        var itemName = reforgedItemMatch.Success ? reforgedItemMatch.Groups[1].Value : name;
        var item = query.FirstOrDefault(i => i.Name.Equals(itemName, StringComparison.InvariantCultureIgnoreCase));

        if (!reforgedItemMatch.Success)
        {
            return item;
        }

        var reforgeLevel = short.Parse(reforgedItemMatch.Groups[2].Value);
        var reforgedItem = item.Reforged(reforgeLevel);

        return reforgedItem;
    }
    
    public IReadOnlyCollection<Item> Search(string name = null, string slot = null, string type = null,
        string quality = null, int? level = null, string boss = null)
    {
        var items = _itemsStore.GetAll().AsQueryable();

        return items.AsQueryable()
            .WhereIf(!string.IsNullOrWhiteSpace(name), item => item.Name.Contains(name, StringComparison.OrdinalIgnoreCase))
            .WhereIf(!string.IsNullOrWhiteSpace(slot), item => item.Slot.Contains(slot, StringComparison.OrdinalIgnoreCase))
            .WhereIf(!string.IsNullOrWhiteSpace(type), item => item.Type.Contains(type, StringComparison.OrdinalIgnoreCase))
            .WhereIf(!string.IsNullOrWhiteSpace(quality), item => item.Quality.Contains(quality, StringComparison.OrdinalIgnoreCase))
            .WhereIf(!string.IsNullOrWhiteSpace(boss), item => item.ObtainableFrom.Contains(boss, StringComparison.OrdinalIgnoreCase))
            .WhereIf(level.HasValue, item => item.Level == level.Value)
            .ToList();
    }
}