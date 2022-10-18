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
    }

    public async Task<IReadOnlyCollection<Item>> GetAllAsync()
    {
        if (_items == null || _items.Any())
        {
            await ReloadItemsAsync();
        }

        return _items;
    }

    public async Task ReloadItemsAsync()
    {
        _items = (await _itemsLoader.LoadAsync()).OrderBy(i => i.Level).ThenBy(i => i.Name).ToList();
    }
}