using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.EventArgs;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.Data.Abstractions;

namespace TkokDiscordBot.Core.Commands
{
    internal class AdministrationCommand : IBotCommand
    {
        private readonly ISettings _settings;
        private readonly IItemsStore _itemsStore;

        public AdministrationCommand(ISettings settings, IItemsStore itemsStore)
        {
            _settings = settings;
            _itemsStore = itemsStore;
        }

        public async Task<bool> Handle(DiscordClient sender, MessageCreateEventArgs eventArgs)
        {
            var commandPrefix = "!admin ";
            if (!eventArgs.Channel.IsPrivate || !eventArgs.Message.Content.StartsWith("!admin "))
            {
                return false;
            }

            //authorize admins
            var admins = _settings.DiscordBotAdmins.Split(',').Select(ulong.Parse);
            if (admins.Contains(eventArgs.Author.Id))
            {
                var commandString = eventArgs.Message.Content.Substring(commandPrefix.Length).Trim();
                var commandArguments = commandString.Split(' ');
                var command = commandArguments[0];

                switch (command)
                {
                    case "set":
                        var key = commandArguments[1];
                        var value = commandArguments[2];
                        switch (key)
                        {
                            case "EntMap":
                                _settings.EntMap = value;
                                await eventArgs.Message.RespondAsync($"{value} setting was updated.");
                                break;
                            case "EntUsername":
                                _settings.EntUsername = value;
                                await eventArgs.Message.RespondAsync($"{value} setting was updated.");
                                break;
                            case "EntPassword":
                                _settings.EntPassword = value;
                                await eventArgs.Message.RespondAsync($"{value} setting was updated.");
                                break;
                            case "DiscordBotAdmins":
                                _settings.DiscordBotAdmins = value;
                                await eventArgs.Message.RespondAsync($"{value} setting was updated.");
                                break;
                        }
                        break;
                    case "sync-items":
                        await _itemsStore.ReSyncItems();
                        var allItems = (await _itemsStore.GetAllAsync()).Count();
                        await eventArgs.Message.RespondAsync($"Items are reloaded. Total items in storage: {allItems}");
                        break;
                    case "debug":
                        var debugInfo = $"EntMap: {_settings.EntMap}\r\n" +
                            $"EntUsername: {_settings.EntUsername}\r\n" +
                            $"EntPassword: {_settings.EntPassword}\r\n" +
                            $"DiscordBotAdmins: {_settings.DiscordBotAdmins}";
                        await eventArgs.Message.RespondAsync($"```{debugInfo}```");
                        break;
                }

                return true;
            }
            return true;
        }

        public CommandInfo GetUsage()
        {
            return null;
        }
    }
}