using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using JetBrains.Annotations;
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

        var configProperty = _settings.GetType().GetProperty(key);
        if (configProperty == null)
        {
            await context.Message.RespondAsync($"Invalid key `{key}`.");
            return;
        }

        configProperty.SetValue(_settings, value, null);
        await context.Message.RespondAsync($"Successfully set `{configProperty.Name}`=`{value}`.");
    }

    [Command("sync-items")]
    public async Task SyncItemsAsync(CommandContext context)
    {
        await _itemsStore.ReloadItemsAsync();

        await context.RespondAsync($"Items are reloaded. Total items in storage: {_itemsStore.GetAll().Count}");
    }
}