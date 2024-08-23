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
using DSharpPlus.Net.WebSocket;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Core.Commands.Attributes;
using TkokDiscordBot.EntGaming;

namespace TkokDiscordBot.Core
{
    internal class Bot
    {
        public const string DefaultMainChannelTopic = "General and Main-discussions - [English]";

        private readonly List<IBotCommand> _botCommands;
        private readonly EntClient _entClient;
        private readonly ISettings _settings;
        private DiscordChannel _mainChannel;

        public DiscordClient Client { get; set; }
        public CommandsNextModule Commands { get; set; }

        public Bot(IEnumerable<IBotCommand> commands, ISettings settings, EntClient entClient)
        {
            _botCommands = commands.OrderBy(c => (short?)c.GetType().GetAttribute<PriorityAttribute>()?.Priority ?? 0)
                .ToList();
            _settings = settings;
            _entClient = entClient;
            _entClient.GameInfoChanged += EntClientOnGameInfoChanged;

            InitializeDiscordClient();
        }

        private void InitializeDiscordClient()
        {
            Client = new DiscordClient(new DiscordConfiguration
            {
                Token = _settings.DiscordToken,
                TokenType = TokenType.Bot,
                LogLevel = LogLevel.Info,
                UseInternalLogHandler = true
            });

            Client.SetWebSocketClient<WebSocket4NetClient>();

            Client.DebugLogger.LogMessage(LogLevel.Info, nameof(Bot), "Initializing events", DateTime.Now);

            Client.MessageCreated += OnMessageCreated;
            Client.GuildMemberAdded += OnGuildMemberAdded;
            Client.GuildAvailable += OnGuildAvailable;
            Client.ClientErrored += OnClientErrored;

            var commandsNextConfig = new CommandsNextConfiguration();
            Commands = Client.UseCommandsNext(commandsNextConfig);

            Client.DebugLogger.LogMessage(LogLevel.Info, nameof(Bot), "Initializing Ready", DateTime.Now);

            Client.Ready += OnReady;

        }

        #region Discord Client Event Handlers

        private async void EntClientOnGameInfoChanged(object sender, PropertyChangedEventArgs eventArgs)
        {
            if (eventArgs.PropertyName != nameof(EntClient.GameInfo)) return;

            var lobbyInfo = _entClient.GameInfo;

            if (lobbyInfo != null)
            {
                //DebugLogger.LogMessage(LogLevel.Info, nameof(Bot), $"Updating topic: {lobbyInfo}", DateTime.Now);

                //await UpdateStatusAsync(new DiscordGame(lobbyInfo.ToString()));
                await _mainChannel.ModifyAsync("main", null, "Currently hosting: " + lobbyInfo);
            }
            else
            {
                //DebugLogger.LogMessage(LogLevel.Info, nameof(Bot), "Updating topic: Default Main Channel Topic", DateTime.Now);

                //await UpdateStatusAsync();
                await _mainChannel.ModifyAsync("main", null, DefaultMainChannelTopic);
            }
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
            _mainChannel = await Client.GetChannelAsync((ulong)_settings.MainChannelId);
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
                foreach (var command in _botCommands)
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