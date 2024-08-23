using System.Collections.Generic;
using TkokDiscordBot.Entities;

namespace TkokDiscordBot.Search;

public interface IFullTextSearchService
{
    IReadOnlyCollection<Item> Search(string query);
}