using System;
using System.Linq;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Autofac.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core;

namespace TkokDiscordBot;

internal class Program
{
    private static async Task Main(string[] args)
    {
        //Setup DI container
        var builder = new ContainerBuilder();
        builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());
        builder.Populate(Enumerable.Empty<ServiceDescriptor>());
        await using var container = builder.Build();

        //Ensure configuration is valid
        EnsureSettingsArePresent(container);

        //Run the bot
        using var bot = container.Resolve<Bot>();

        await bot.Client.ConnectAsync();
        await Task.Delay(-1);
    }

    private static void EnsureSettingsArePresent(IContainer container)
    {
        var settings = container.Resolve<ISettings>();

        if (string.IsNullOrWhiteSpace(settings.DiscordToken))
        {
            throw new Exception("Couldn't find Discord token. Ensure that \"user-secrets.json\" is present and contains value for DiscordToken.");
        }
    }
}