using System.Collections.Generic;
using TkokDiscordBot.Entities;

namespace TkokDiscordBot.Data.Abstractions;

public interface IItemsRepository
{
    IReadOnlyCollection<Item> GetAll();

    Item Get(string name);

    IReadOnlyCollection<Item> Search(string name = null, string slot = null, string type = null, string quality = null, int? level = null, string boss = null);
}