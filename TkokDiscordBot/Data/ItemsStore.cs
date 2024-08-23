using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Entities;

namespace TkokDiscordBot.Data;

public class ItemsStore : IItemsStore
{
    private IReadOnlyCollection<Item> _items;
    private readonly IItemsLoader _itemsLoader;

    public ItemsStore(IItemsLoader itemsLoader)
    {
        _itemsLoader = itemsLoader;

        ReloadItems();
    }

    public event EventHandler<IReadOnlyCollection<Item>> ItemsReloaded;

    public IReadOnlyCollection<Item> GetAll() => _items;

    public void ReloadItems()
    {
        var items = _itemsLoader.Load();
        _items = items.OrderBy(i => i.Level).ThenBy(i => i.Name).ToList();
    }

    public async Task ReloadItemsAsync()
    {
        var items = await _itemsLoader.LoadAsync();
        _items = items.OrderBy(i => i.Level).ThenBy(i => i.Name).ToList();
        OnItemsReloaded(_items);
    }

    protected virtual void OnItemsReloaded(IReadOnlyCollection<Item> items)
    {
        ItemsReloaded?.Invoke(this, items);
    }
}