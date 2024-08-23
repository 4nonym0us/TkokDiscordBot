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
        private static Bot _bot;
        private static IContainer _container;

        private static async Task Main(string[] args)
        {
            //Setup DI container
            var builder = new ContainerBuilder();
            builder.RegisterAssemblyModules(Assembly.GetExecutingAssembly());
            _container = builder.Build();

            //Ensure configuration is valid
            EnsureSettingsArePresent();

            AppDomain.CurrentDomain.ProcessExit += OnCurrentDomainOnProcessExit;

            //Run the bot
            _bot = _container.Resolve<Bot>();

            await _bot.Client.ConnectAsync();
            await Task.Delay(-1);
        }

        private static async void OnCurrentDomainOnProcessExit(object sender, EventArgs eventArgs)
        {
            if (_bot?.Client != null)
            {
                await _bot?.Client?.DisconnectAsync();
            }

            _container.Dispose();
        }

        private static void EnsureSettingsArePresent()
        {
            using var scope = _container.BeginLifetimeScope();
            var settings = scope.Resolve<ISettings>();
            if (string.IsNullOrWhiteSpace(settings.DiscordToken))
            {
                throw new Exception("Couldn't find Discord token. Ensure that \"user-secrets.json\" is present and contains value for DiscordToken.");
            }
            scope.Dispose();
        }
    }
}