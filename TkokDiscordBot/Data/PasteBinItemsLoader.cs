using System.Collections.Generic;
using System.IO;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.Entities;

namespace TkokDiscordBot.Data;

public class PasteBinItemsLoader : ItemLoaderBase
{
    private const string PasteBinUrl = "https://pastebin.com/raw/93Q7pjQJ";

    public override async Task<IEnumerable<Item>> LoadAsync()
    {
        using var client = new HttpClient();
        var response = await client.GetStringAsync(PasteBinUrl);
        var fileLines = Regex.Split(response, @"\r?\n");

        return ParseLines(fileLines);
    }

    public override IEnumerable<Item> Load()
    {
        using var client = new HttpClient();
        using var response = client.Send(new HttpRequestMessage(HttpMethod.Get, PasteBinUrl));
        using var reader = new StreamReader(response.Content.ReadAsStream());

        var fileLines = Regex.Split(reader.ReadToEnd(), @"\r?\n");

        return ParseLines(fileLines);
    }
}