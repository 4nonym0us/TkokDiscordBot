using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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
        private readonly Regex _reforgedItemRegex = new Regex(@"^(.*?)\s*?\+([1-9]|1[0-5])$", RegexOptions.Compiled);

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
            //var query = await _itemsStore.GetAllAsync();
            //return query.FirstOrDefault(item => item.Name.Equals(name, StringComparison.InvariantCultureIgnoreCase));

            var query = await _itemsStore.GetAllAsync();
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

            if (!string.IsNullOrWhiteSpace(filter))
            {
                query = query.Where(item =>
                    item.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.Slot.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.Type.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.Quality.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    item.ObtainableFrom.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            query = query.WhereIf(level.HasValue, item => item.Level == level.Value);
            query = query.WhereIf(@class != TkokClass.None, item => TkokClassHelper.GetPredicateForItemLookup(@class).Invoke(item));

            return query;
        }
    }
}