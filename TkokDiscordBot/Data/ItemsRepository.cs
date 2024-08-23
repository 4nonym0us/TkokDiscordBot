using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Entities;
using TkokDiscordBot.Enums;
using TkokDiscordBot.Extensions;
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

        public async Task<Item> FirstOrDefault(Func<Item, bool> predicate)
        {
            var query = await _itemsStore.GetAllAsync();
            return query.FirstOrDefault(predicate);
        }

        public async Task<IEnumerable<Item>> Search(string name = null, string slot = null, string type = null, string quality = null, int? level = null, string boss = null)
        {
            var query = (await _itemsStore.GetAllAsync()).AsQueryable();

            query = query.WhereIf(!string.IsNullOrWhiteSpace(name),
                item => item.Name.IndexOf(name, StringComparison.OrdinalIgnoreCase) >= 0);

            query = query.WhereIf(!string.IsNullOrWhiteSpace(slot),
                item => item.Slot.IndexOf(slot, StringComparison.OrdinalIgnoreCase) >= 0);

            query = query.WhereIf(!string.IsNullOrWhiteSpace(type),
                item => item.Type.IndexOf(type, StringComparison.OrdinalIgnoreCase) >= 0);

            query = query.WhereIf(!string.IsNullOrWhiteSpace(quality),
                item => item.Quality.IndexOf(quality, StringComparison.OrdinalIgnoreCase) >= 0);

            query = query.WhereIf(!string.IsNullOrWhiteSpace(boss),
                item => item.ObtainableFrom.IndexOf(boss, StringComparison.OrdinalIgnoreCase) >= 0);

            query = query.WhereIf(level.HasValue, item => item.Level == level.Value);

            return query;
        }

        public async Task<IEnumerable<Item>> FullTextSearch(string filter, int? level, TkokClass @class)
        {
            var query = (await _itemsStore.GetAllAsync()).AsQueryable();
            IQueryable<Item> foundItems = new EnumerableQuery<Item>(Expression.Empty());

            if (!string.IsNullOrWhiteSpace(filter))
            {
                foundItems = query.Where(item =>
                    item.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.Slot.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.Type.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.Quality.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.ObtainableFrom.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            foundItems = foundItems.WhereIf(level.HasValue, item => item.Level == level.Value);
            foundItems = foundItems.WhereIf(@class != TkokClass.None, item => TkokClassHelper.GetPredicateForItemLookup(@class).Invoke(item));

            return foundItems;
        }
    }
}