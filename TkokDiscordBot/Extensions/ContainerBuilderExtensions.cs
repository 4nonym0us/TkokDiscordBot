using System;
using System.Linq;
using System.Reflection;
using Autofac;
using TkokDiscordBot.Core.Commands.Abstractions;

namespace TkokDiscordBot.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterCommands(this ContainerBuilder builder, Assembly assembly)
        {
            var cmdInterface = typeof(IBotCommand);
            var types = assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && cmdInterface.IsAssignableFrom(type))
                .ToList();

            foreach (var type in types)
            {
                builder.RegisterType(type).As<IBotCommand>().PreserveExistingDefaults();
            }

            Console.WriteLine($"Loaded {types.Count} commands.");
        }
    }
}
