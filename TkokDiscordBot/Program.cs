using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using Config.Net;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Data;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.EntGaming;
using TkokDiscordBot.Extensions;

namespace TkokDiscordBot
{
    internal class Program
    {
        private static IContainer Container { get; set; }

        private static void Main(string[] args)
        {
            var settings = new ConfigurationBuilder<ISettings>()
                .UseAppConfig()
                .UseJsonFile("user-secrets.json")
                .Build();

            //Setup DI container
            var builder = new ContainerBuilder();
            builder.RegisterType<PasteBinItemsLoader>().As<IItemsLoader>(); //PasteBinItemsLoader|LocalFileItemsLoader
            builder.RegisterType<ItemsStore>().As<IItemsStore>().SingleInstance();
            builder.RegisterType<ItemsRepository>().As<IItemsRepository>();
            builder.RegisterType<Bot>().AsSelf();
            builder.RegisterType<EntClient>().AsSelf().SingleInstance();

            builder.RegisterCommands(Assembly.GetAssembly(typeof(IBotCommand)));

            builder.Register(c => settings).As<ISettings>().SingleInstance();

            Container = builder.Build();

            EnsureSettingsArePresent(settings);

            //Run the bot
            MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            var bot = Container.Resolve<Bot>();

            await bot.Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static void EnsureSettingsArePresent(ISettings settings)
        {
            if (string.IsNullOrWhiteSpace(settings.DiscordToken))
            {
                throw new Exception("Couldn't find Discord token. Ensure that \"user-secrets.json\" is present and contains value for DiscordToken.");
            }
        }
    }
}