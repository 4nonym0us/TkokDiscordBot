using System;
using System.Linq;
using Autofac;

namespace TkokDiscordBot.Extensions
{
    public static class ContainerBuilderExtensions
    {
        public static void RegisterCommands<TType>(this ContainerBuilder builder)
        {
            var cmdInterface = typeof(TType);
            var assembly = cmdInterface.Assembly;
            var types = assembly.GetTypes()
                .Where(type => type.IsClass && !type.IsAbstract && cmdInterface.IsAssignableFrom(type))
                .ToList();

            foreach (var type in types)
            {
                builder.RegisterType(type).As<TType>().PreserveExistingDefaults();
            }

            Console.WriteLine($"Dynamically loaded {types.Count} {cmdInterface.Name} commands.");
        }
    }
}
