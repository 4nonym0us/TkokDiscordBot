using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using JetBrains.Annotations;
using System.Globalization;
using System.Threading.Tasks;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Data.Abstractions;

namespace TkokDiscordBot.Core.CommandsNext;

[Hidden]
[Group("admin")]
[UsedImplicitly]
[RequirePermissions(Permissions.ManageChannels)]
public class AdministrationCommands : BaseCommandModule
{
    private readonly ISettings _settings;
    private readonly IItemsStore _itemsStore;

    public AdministrationCommands(ISettings settings, IItemsStore itemsStore)
    {
        _settings = settings;
        _itemsStore = itemsStore;
    }

    [Command("set")]
    public async Task SetAsync(CommandContext context, string key, string value)
    {
        if (!context.Channel.IsPrivate)
        {
            await context.Message.RespondAsync("Current command is available only in Private channel.");
            return;
        }

        if (_settings is Settings settings)
        {
            switch (key.ToLower())
            {
                case nameof(ISettings.DiscordToken):
                    settings.DiscordToken = value;
                    break;
                case nameof(ISettings.BotCommandsChannelId):
                    if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var botCommandsChannelId))
                    {
                        settings.BotCommandsChannelId = botCommandsChannelId;
                    }
                    else
                    {
                        await context.Message.RespondAsync($"Failed to parse value `{value}`.");
                        return;
                    }
                    break;
                case nameof(ISettings.MainServerId):
                    if (ulong.TryParse(value, NumberStyles.None, CultureInfo.InvariantCulture, out var mainServerId))
                    {
                        settings.MainServerId = mainServerId;
                    }
                    else
                    {
                        await context.Message.RespondAsync($"Failed to parse value `{value}`.");
                        return;
                    }
                    break;
                default:
                    await context.Message.RespondAsync($"Invalid key `{key}`.");
                    break;
            }
        }

        await context.Message.RespondAsync($"Successfully set `{key}`=`{value}` (value is reset on app restart; update config file to make permanent changes).");
    }

    [Command("sync-items")]
    public async Task SyncItemsAsync(CommandContext context)
    {
        await _itemsStore.ReloadItemsAsync();

        await context.RespondAsync($"Items are reloaded. Total items in storage: {_itemsStore.GetAll().Count}");
    }
}