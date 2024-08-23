using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Entities;

namespace TkokDiscordBot.Data
{
    public class PasteBinItemsLoader : ItemLoaderBase
    {
        public override async Task<IEnumerable<Item>> Load()
        {
            using (var client = new HttpClient())
            {
                var response = await client.GetStringAsync("https://pastebin.com/raw/93Q7pjQJ");
                var fileLines = Regex.Split(response, "\r\n|\r|\n");

                return ParseLines(fileLines);
            }
        }
    }
}