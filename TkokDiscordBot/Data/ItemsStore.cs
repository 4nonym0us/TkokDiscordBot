using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Entities;

namespace TkokDiscordBot.Data
{
    public class ItemsStore : IItemsStore
    {
        private IEnumerable<Item> _items;
        private readonly IItemsLoader _itemsLoader;

        public ItemsStore(IItemsLoader itemsLoader)
        {
            _itemsLoader = itemsLoader;
        }

        public async Task<IEnumerable<Item>> GetAllAsync()
        {
            if (_items == null || _items.Any())
            {
                await ReSyncItems();
            }
            return _items;
        }

        public async Task ReSyncItems()
        {
            _items = await _itemsLoader.Load();
        }
    }
}