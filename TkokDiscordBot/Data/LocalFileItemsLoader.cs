using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Entities;

namespace TkokDiscordBot.Data
{
    public class LocalFileItemsLoader : ItemLoaderBase
    {
        /// <summary>
        /// Loads items from `droplist.txt`.
        /// </summary>
        /// <returns></returns>
        public override Task<IEnumerable<Item>> Load()
        {
            var fileData = File.ReadAllLines(@"droplist.txt");

            return Task.FromResult(ParseLines(fileData));
        }
    }
}