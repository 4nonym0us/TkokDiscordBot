using System.Collections.Generic;
using System.Threading.Tasks;
using TkokDiscordBot.Entities;
using TkokDiscordBot.Enums;

namespace TkokDiscordBot.Data.Abstractions;

public interface IItemsRepository
{
    Task<IReadOnlyCollection<Item>> GetAllAsync();

    Task<Item> GetAsync(string name);

    Task<IReadOnlyCollection<Item>> SearchAsync(string name = null, string slot = null, string type = null, string quality = null, int? level = null, string boss = null);

    Task<IReadOnlyCollection<Item>> FullTextSearchAsync(string filter, int? level, TkokClass @class);
}