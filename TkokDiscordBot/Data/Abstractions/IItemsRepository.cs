using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TkokDiscordBot.Entities;
using TkokDiscordBot.Enums;

namespace TkokDiscordBot.Data.Abstractions
{
    public interface IItemsRepository
    {
        Task<IEnumerable<Item>> GetAll();
        Task<Item> Get(string name);
        Task<IEnumerable<Item>> Search(string name = null, string slot = null, string type = null, string quality = null, int? level = null, string boss = null);
        Task<IEnumerable<Item>> FullTextSearch(string filter, int? level, TkokClass @class);
        Task<Item> FirstOrDefault(Func<Item, bool> predicate);
    }
}