using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Entities;
using TkokDiscordBot.Enums;
using TkokDiscordBot.Helpers;

namespace TkokDiscordBot.Data
{
    public class ItemsRepository : IItemsRepository
    {
        private readonly IItemsStore _itemsStore;

        public ItemsRepository(IItemsStore itemsStore)
        {
            _itemsStore = itemsStore;
        }

        public async Task<IEnumerable<Item>> GetAll()
        {
            var query = await _itemsStore.GetAllAsync();
            return query;
        }

        public async Task<Item> Get(string name)
        {
            var query = await _itemsStore.GetAllAsync();
            return query.FirstOrDefault(item => item.Name == name);
        }

        public async Task<IEnumerable<Item>> Search(string name = null, string slot = null, string type = null, string quality = null, int? level = null, string boss = null)
        {
            var query = await _itemsStore.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(name))
            {
                query = query.Where(item => item.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!string.IsNullOrWhiteSpace(slot))
            {
                query = query.Where(item => item.Slot.IndexOf(slot, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!string.IsNullOrWhiteSpace(type))
            {
                query = query.Where(item => item.Type.IndexOf(type, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (!string.IsNullOrWhiteSpace(quality))
            {
                query = query.Where(item => item.Quality.IndexOf(quality, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            if (level.HasValue)
            {
                query = query.Where(item => item.Level == level.Value);
            }

            if (!string.IsNullOrWhiteSpace(boss))
            {
                query = query.Where(item => item.ObtainableFrom.IndexOf(boss, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            return query;
        }

        public async Task<IEnumerable<Item>> FullTextSearch(string filter, int? level, TkokClass @class)
        {
            var query = await _itemsStore.GetAllAsync();

            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(item => item.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.Slot.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.Type.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.Quality.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.ObtainableFrom.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0
                    //filter.IndexOf(item.Slot, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    //filter.IndexOf(item.Type, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    //filter.IndexOf(item.Quality, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    //filter.IndexOf(item.ObtainableFrom, StringComparison.OrdinalIgnoreCase) >= 0
                    );
            }

            if (level.HasValue)
            {
                query = query.Where(item => item.Level == level.Value);
            }

            if (@class != TkokClass.None)
            {
                query = query.Where(TkokClassHelper.GetPredicateForItemLookup(@class));
            }

            return query;
        }
    }
}