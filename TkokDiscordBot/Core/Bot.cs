using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Castle.Core.Internal;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Interactivity;
using DSharpPlus.Net.WebSocket;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Attributes;
using TkokDiscordBot.Core.CommandsNext;
using TkokDiscordBot.Data.Abstractions;
using TkokDiscordBot.EntGaming;

namespace TkokDiscordBot.Core
{
    internal class Bot
    {
        private readonly List<IBotCommand> _botCommands;
        private readonly IEnumerable<ICommandNext> _commandsNext;
        private readonly EntClient _entClient;
        private readonly IItemsRepository _itemsRepository;
        private readonly IItemsStore _itemsStore;
        private readonly ISettings _settings;
        private string _defaultMainChannelTopic = "General and Main-discussions - [English]";
        private DiscordChannel _mainChannel;

        public Bot(
            IEnumerable<IBotCommand> commands,
            IEnumerable<ICommandNext> commandsNext,
            ISettings settings,
            EntClient entClient,
            IItemsRepository itemsRepository,
            IItemsStore itemsStore)
        {
            _botCommands = commands.OrderBy(c => (short?) c.GetType().GetAttribute<PriorityAttribute>()?.Priority ?? 0).ToList();
            _commandsNext = commandsNext;
            _settings = settings;
            _entClient = entClient;
            _itemsRepository = itemsRepository;
            _itemsStore = itemsStore;
            _entClient.GameInfoChanged += EntClientOnGameInfoChanged;

            InitializeDiscordClient();
            InitializeCommandsNext();
            InitializeInteractivity();

            Client.DebugLogger.LogMessage(LogLevel.Info, nameof(Bot), "Initializing Ready", DateTime.Now);
        }

        public DiscordClient Client { get; set; }
        public InteractivityModule Interactivity { get; set; }
        public CommandsNextModule Commands { get; set; }

        #region DiscordClient/CommandNext/Interactivity Initialization

        private void InitializeDiscordClient()
        {
            Client.DebugLogger.LogMessage(LogLevel.Debug, nameof(InitializeDiscordClient), "Starting...", DateTime.Now);

            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = _settings.DiscordToken,
                TokenType = TokenType.Bot,
                LogLevel = LogLevel.Info,
                UseInternalLogHandler = true
            });

            Client.SetWebSocketClient<WebSocket4NetClient>();

            Client.MessageCreated += OnMessageCreated;
            Client.GuildMemberAdded += OnGuildMemberAdded;
            Client.GuildAvailable += OnGuildAvailable;
            Client.ClientErrored += OnClientErrored;

            Client.Ready += OnReady;

            Client.DebugLogger.LogMessage(LogLevel.Debug, nameof(InitializeDiscordClient), "Done.", DateTime.Now);
        }

        private void InitializeCommandsNext()
        {
            Client.DebugLogger.LogMessage(LogLevel.Debug, nameof(InitializeCommandsNext), "Starting...", DateTime.Now);

            //TODO: find a non-retarded way to make Autofac handle CommandsNext DI
            var dependencyCollectionBuilder = new DependencyCollectionBuilder();
            dependencyCollectionBuilder.AddInstance(_settings);
            dependencyCollectionBuilder.AddInstance(_entClient);
            dependencyCollectionBuilder.AddInstance(_itemsRepository);
            dependencyCollectionBuilder.AddInstance(_itemsStore);

            var commandsNextConfig = new CommandsNextConfiguration
            {
                StringPrefix = "!",
                Dependencies = dependencyCollectionBuilder.Build(),
                EnableDefaultHelp = false
            };
            Commands = Client.UseCommandsNext(commandsNextConfig);
            Commands.RegisterCommands<PingCommand>();
            Commands.RegisterCommands<TrackCommand>();
            Commands.RegisterCommands<GameHostingCommand>();
            Commands.RegisterCommands<AdministrationCommands>();

            Client.DebugLogger.LogMessage(LogLevel.Debug, nameof(InitializeCommandsNext), "Done.", DateTime.Now);
        }

        private void InitializeInteractivity()
        {
            Client.DebugLogger.LogMessage(LogLevel.Debug, nameof(InitializeInteractivity), "Starting...", DateTime.Now);

            Interactivity = Client.UseInteractivity(new InteractivityConfiguration
            {
                PaginationBehaviour =
                    TimeoutBehaviour.Ignore, // default pagination behaviour to just ignore the reactions
                PaginationTimeout = TimeSpan.FromMinutes(5), // default pagination timeout to 5 minutes
                Timeout = TimeSpan.FromMinutes(2) // default timeout for other actions to 2 minutes
            });

            Client.DebugLogger.LogMessage(LogLevel.Debug, nameof(InitializeInteractivity), "Done.", DateTime.Now);
        }

        #endregion

        #region Discord Client Event Handlers

        private async void EntClientOnGameInfoChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName != nameof(EntClient.GameInfo)) return;

            var lobbyInfo = _entClient.GameInfo;

            if (lobbyInfo != null)
                await _mainChannel.ModifyAsync("main", null, "Currently hosting: " + lobbyInfo);
            else
                await _mainChannel.ModifyAsync("main", null, _defaultMainChannelTopic);
        }

        private Task OnClientErrored(ClientErrorEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Error, nameof(Bot),
                $"Exception occured: {e.Exception.GetType()}: {e.Exception.Message}", DateTime.Now);
            return Task.CompletedTask;
        }

        private Task OnGuildAvailable(GuildCreateEventArgs e)
        {
            e.Client.DebugLogger.LogMessage(LogLevel.Info, nameof(Bot), $"Guild available: {e.Guild.Name}",
                DateTime.Now);
            return Task.CompletedTask;
        }

        private async Task OnReady(ReadyEventArgs e)
        {
            await Task.Yield();
            _mainChannel = await Client.GetChannelAsync((ulong) _settings.MainChannelId);
            _defaultMainChannelTopic = _mainChannel.Topic;
            Client.DebugLogger.LogMessage(LogLevel.Info, nameof(Bot), "Ready! Setting status message..", DateTime.Now);

            await Client.UpdateStatusAsync(new DiscordGame("Creating SkyNet"));
        }

        private async Task OnGuildMemberAdded(GuildMemberAddEventArgs e)
        {
            var lfgChannel = await Client.GetChannelAsync(220901817305923584);
            var welcomeText = $"{e.Member.Mention} :smiley: Welcome to the **Official TKoK** server! " +
                              $"I\'m TKoK Bot <:bot:379752914215763994> and if you want to check out my features, " +
                              $"type `!help` or `!commands`. Visit {lfgChannel.Mention} channel to find a teammates.";

            await _mainChannel.SendMessageAsync(welcomeText);
        }

        private async Task OnMessageCreated(MessageCreateEventArgs e)
        {
            //bot shouldn't handle commands of other bots or itself
            if (e.Author.IsBot)
                return;

            if (Regex.IsMatch(e.Message.Content.Trim(), @"^!(help|commands|cmds|cmd|h|\?)$"))
            {
                var builder = new DiscordEmbedBuilder();
                builder.WithColor(DiscordColor.Azure);
                foreach (var command in _botCommands)
                {
                    var usage = command.GetUsage();
                    if (usage != null)
                        builder.AddField(usage.Command, usage.Usage);
                }
                foreach (var command in _commandsNext)
                {
                    var usage = command.GetUsage();
                    if (usage != null)
                        builder.AddField(usage.Command, usage.Usage);
                }
                await e.Message.RespondAsync("List of supported commands", false, builder.Build());

                return;
            }

            foreach (var command in _botCommands)
            {
                var handled = await command.Handle(Client, e);
                if (handled)
                {
                    Client.DebugLogger.LogMessage(LogLevel.Info, nameof(Bot),
                        $"Command {e.Author.Username}:'{e.Message.Content}' was handled by {command.GetType().Name}",
                        DateTime.Now);
                    break; //ensure that we don't handle the same message with multiple commands
                }
            }
        }

        #endregion
    }
}