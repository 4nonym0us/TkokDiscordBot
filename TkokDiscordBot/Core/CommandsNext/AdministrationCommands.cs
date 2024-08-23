using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.Data.Abstractions;

namespace TkokDiscordBot.Core.CommandsNext
{
    [Hidden]
    [Group("admin", CanInvokeWithoutSubcommand = false)]
    [RequirePermissions(Permissions.ManageChannels)]
    internal class AdministrationCommands : ICommandNext
    {
        private readonly ISettings _settings;
        private readonly IItemsStore _itemsStore;

        public AdministrationCommands(ISettings settings, IItemsStore itemsStore)
        {
            _settings = settings;
            _itemsStore = itemsStore;
        }

        [Command("set")]
        public async Task Set(CommandContext context, string key, string value)
        {
            if (!context.Channel.IsPrivate)
            {
                await context.Message.RespondAsync("Current command is available only in Private channel.");
                return;
            }

            var configProperty = _settings.GetType().GetProperty(key);
            if (configProperty == null)
            {
                await context.Message.RespondAsync($"Invalid key `{key}`.");
                return;
            }

            configProperty.SetValue(_settings, value, null);
            await context.Message.RespondAsync($"Successfully set `{configProperty.Name}`=`{value}`.");
        }

        [Command("debug")]
        public async Task Debug(CommandContext context)
        {
            if (!context.Channel.IsPrivate)
            {
                await context.Message.RespondAsync("Current command is available only in Private channel.");
                return;
            }

            var debugInfo = $"EntMap: {_settings.EntMap}\r\n" +
                            $"EntUsername: {_settings.EntUsername}\r\n" +
                            $"EntPassword: {_settings.EntPassword}\r\n" +
                            $"DiscordBotAdmins: {_settings.DiscordBotAdmins}";
            await context.RespondAsync($"```{debugInfo}```");
        }

        [Command("sync-items")]
        public async Task SyncItems(CommandContext context)
        {
            await _itemsStore.ReSyncItems();
            var allItems = (await _itemsStore.GetAllAsync()).Count();
            await context.RespondAsync($"Items are reloaded. Total items in storage: {allItems}");
        }

        public CommandInfo GetUsage()
        {
            return null;
        }
    }
}