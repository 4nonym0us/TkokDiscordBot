using System.Threading.Tasks;
using TkokDiscordBot.Core;
using TkokDiscordBot.Dependency;

namespace TkokDiscordBot;

internal class Program
{
    private static async Task Main()
    {
        //Setup DI container
        await using var scope = new AppServiceProvider();

        //Run the bot
        using var bot = scope.GetService<Bot>();

        await bot.Client.ConnectAsync();
        await Task.Delay(-1);
    }
}