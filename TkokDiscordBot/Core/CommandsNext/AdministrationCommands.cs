using System.Linq;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Attributes;
using JetBrains.Annotations;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.Data.Abstractions;

namespace TkokDiscordBot.Core.CommandsNext;

[Hidden]
[Group("admin")]
[UsedImplicitly]
[RequirePermissions(Permissions.ManageChannels)]
public class AdministrationHasCommands : BaseCommandModule
{
    private readonly ISettings _settings;
    private readonly IItemsStore _itemsStore;

    public AdministrationHasCommands(ISettings settings, IItemsStore itemsStore)
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

    [Command("sync-items")]
    public async Task SyncItems(CommandContext context)
    {
        await _itemsStore.ReloadItemsAsync();
        var allItems = (await _itemsStore.GetAllAsync()).Count();
        await context.RespondAsync($"Items are reloaded. Total items in storage: {allItems}");

        var items = await _itemsStore.GetAllAsync();

        await context.RespondAsync(string.Join(",", items.Select(i => i.Slot).Distinct()));
        await context.RespondAsync(string.Join(",", items.Select(i => i.Type).Distinct()));
        await context.RespondAsync(string.Join(",", items.Select(i => i.Quality).Distinct()));
        await context.RespondAsync(string.Join(",", items.Select(i => i.ObtainableFrom).Distinct()));
        await context.RespondAsync(string.Join(",", items.Select(i => i.ClassRestriction).Distinct()));
    }

    public CommandInfo GetUsage()
    {
        return null;
    }
}