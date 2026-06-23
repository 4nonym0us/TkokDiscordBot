using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.CommandsNext.Exceptions;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Interactivity.Enums;
using DSharpPlus.Interactivity.Extensions;
using Microsoft.Extensions.Logging;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.CommandsNext;

namespace TkokDiscordBot.Core;

public class Bot : IDisposable
{
    private readonly IReadOnlyList<IBotCommand> _botCommands;
    private readonly IReadOnlyList<IHasCommandUsage> _commandUsages;
    private readonly ISettings _settings;
    private bool _disposed;

    public const char CommandPrefix = '!';

    public DiscordClient Client { get; private set; }

    public Bot(IEnumerable<IBotCommand> commands, IEnumerable<IHasCommandUsage> commandsUsageList,
        ISettings settings, IServiceProvider serviceProvider)
    {
        _botCommands = commands.OrderBy(c => c.Priority).ToList();
        _commandUsages = commandsUsageList.OrderBy(ci => ci.GetUsage().Order).ToList();
        _settings = settings;

        InitializeDiscordClient();
        InitializeCommandsNext(serviceProvider);
        InitializeInteractivity();

        Client.Logger.LogInformation("Initialization sequence completed.");
    }

    #region DiscordClient/CommandNext/Interactivity Initialization

    [MemberNotNull(nameof(Client))]
    private void InitializeDiscordClient()
    {
        Client = new DiscordClient(new DiscordConfiguration
        {
            Token = _settings.DiscordToken,
            TokenType = TokenType.Bot,
            MinimumLogLevel = LogLevel.Information,
            Intents = DiscordIntents.AllUnprivileged | DiscordIntents.MessageContents
        });

        Client.ComponentInteractionCreated += OnComponentInteractionCreated;
        Client.MessageCreated += OnMessageCreated;
        Client.GuildAvailable += OnGuildAvailable;
        Client.ClientErrored += OnClientErrored;
        Client.Ready += OnReady;

        Client.Logger.LogDebug("DiscordClient initialization completed.");
    }

    private void InitializeCommandsNext(IServiceProvider services)
    {
        var config = new CommandsNextConfiguration
        {
            StringPrefixes = [new string(CommandPrefix, 1)],
            Services = services,
            EnableDefaultHelp = false,
            IgnoreExtraArguments = true
        };
        var commands = Client.UseCommandsNext(config);
        commands.RegisterCommands(Assembly.GetExecutingAssembly());

        commands.CommandErrored += OnCommandErrored;

        Client.Logger.LogDebug("CommandsNext module has been initialized.");
    }

    private void InitializeInteractivity()
    {
        Client.UseInteractivity(new InteractivityConfiguration
        {
            PaginationBehaviour = PaginationBehaviour.WrapAround,
            Timeout = TimeSpan.FromMinutes(5),
            ButtonBehavior = ButtonPaginationBehavior.Disable
        });

        Client.Logger.LogDebug("Interactivity extension has been initialized.");
    }

    #endregion

    #region Discord Client Event Handlers

    private static async Task OnComponentInteractionCreated(DiscordClient client, ComponentInteractionCreateEventArgs args)
    {
        // Use deferred message update interaction response in Search Wizard
        if (args.Id.StartsWith(SearchWizardCommandBase.IdPrefix))
        {
            await args.Interaction.CreateResponseAsync(InteractionResponseType.DeferredMessageUpdate);
        }
    }

    private Task OnCommandErrored(CommandsNextExtension commandsNext, CommandErrorEventArgs args)
    {
        if (args.Exception is CommandNotFoundException)
        {
            return Task.CompletedTask;
        }

        commandsNext.Client.Logger.LogError($"An exception occurred while handling command: {args.Exception.Message}");
        return Task.CompletedTask;
    }

    private Task OnClientErrored(DiscordClient client, ClientErrorEventArgs e)
    {
        client.Logger.LogError($"Exception occurred: {e.Exception.GetType()}: {e.Exception.Message}");
        return Task.CompletedTask;
    }

    private Task OnGuildAvailable(DiscordClient client, GuildCreateEventArgs e)
    {
        var permissionString = (e.Guild.Permissions ?? Permissions.None).ToPermissionString();
        client.Logger.LogInformation($"Guild available: {e.Guild.Name}, permissions: {permissionString}.");
        return Task.CompletedTask;
    }

    private async Task OnReady(DiscordClient client, ReadyEventArgs e)
    {
        client.Logger.LogInformation("Ready! Setting status message...");

        await client.UpdateStatusAsync(new DiscordActivity("Creating SkyNet"));
    }

    private async Task OnMessageCreated(DiscordClient client, MessageCreateEventArgs args)
    {
        // Bot shouldn't handle commands of other bots or itself
        if (args.Author.IsBot)
        {
            return;
        }

        // Custom `!help` command handler
        if (Regex.IsMatch(args.Message.Content.Trim(), @"^!(help|commands|cmds|cmd|h|\?)$", RegexOptions.Compiled | RegexOptions.IgnoreCase))
        {
            var builder = new DiscordEmbedBuilder();
            builder.WithColor(DiscordColor.Azure);
            foreach (var command in _commandUsages)
            {
                var usage = command.GetUsage();
                builder.AddField(usage.Command, usage.Usage);
            }
            await args.Message.RespondAsync("List of supported commands", builder);

            return;
        }

        // Handle chat messages by custom bot commands
        foreach (var command in _botCommands)
        {
            var handled = await command.HandleAsync(client, args);

            if (handled)
            {
                client.Logger.LogInformation($"Command {args.Author.Username}:'{args.Message.Content}' was handled by {command.GetType().Name}");
                break; // Ensure that we don't handle the same message with multiple commands
            }
        }
    }

    #endregion

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        if (disposing)
        {
            Client.ComponentInteractionCreated -= OnComponentInteractionCreated;
            Client.MessageCreated -= OnMessageCreated;
            Client.GuildAvailable -= OnGuildAvailable;
            Client.ClientErrored -= OnClientErrored;
            Client.Ready -= OnReady;

            Client.Dispose();
        }

        _disposed = true;
    }
}