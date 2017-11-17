using System;
using System.Reflection;
using System.Threading.Tasks;
using Autofac;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core;

namespace TkokDiscordBot
{
    internal class Program
    {
        private static IContainer Container { get; set; }

        private static void Main(string[] args)
        {
            //Setup DI container
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());
            Container = builder.Build();

            //Ensure configuration is valid
            EnsureSettingsArePresent();

            //Run the bot
            MainAsync().ConfigureAwait(false).GetAwaiter().GetResult();
        }

        private static async Task MainAsync()
        {
            var bot = Container.Resolve<Bot>();

            await bot.Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static void EnsureSettingsArePresent()
        {
            using (var scope = Container.BeginLifetimeScope())
            {
                var settings = scope.Resolve<ISettings>();
                if (string.IsNullOrWhiteSpace(settings.DiscordToken))
                {
                    throw new Exception("Couldn't find Discord token. Ensure that \"user-secrets.json\" is present and contains value for DiscordToken.");
                }
                scope.Dispose();
            }
        }
    }
}