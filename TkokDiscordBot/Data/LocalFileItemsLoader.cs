using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Entities;

namespace TkokDiscordBot.Data;

public class LocalFileItemsLoader : ItemLoaderBase
{
    /// <summary>
    /// Loads items from `droplist.txt`.
    /// </summary>
    /// <returns></returns>
    public override async Task<IEnumerable<Item>> LoadAsync()
    {
        var fileData = await File.ReadAllLinesAsync(@"droplist.txt");

        return ParseLines(fileData);
    }

    /// <summary>
    /// Loads items from `droplist.txt`.
    /// </summary>
    /// <returns></returns>
    public override IEnumerable<Item> Load()
    {
        var fileData = File.ReadAllLines(@"droplist.txt");

        return ParseLines(fileData);
    }
}