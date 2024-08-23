using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TkokDiscordBot.Entities;

namespace TkokDiscordBot.Data.Abstractions;

public interface IItemsStore
{
    IReadOnlyCollection<Item> GetAll();

    Task ReloadItemsAsync();

    event EventHandler<IReadOnlyCollection<Item>> ItemsReloaded;
}