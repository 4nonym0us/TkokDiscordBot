using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using JetBrains.Annotations;
using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Enums;

namespace TkokDiscordBot.Core.Commands;

/// <summary>
/// Honeypot channel handler, which bans automated spammers on sight.
/// </summary>
/// <remarks>
/// Requires configured <see cref="Settings.HoneypotChannelId"/>.
/// </remarks>
[Hidden]
[UsedImplicitly]
public class HoneypotCommand(ISettings settings) : IBotCommand
{
    public CommandPriority Priority => CommandPriority.Medium;

    public async Task<bool> HandleAsync(DiscordClient client, MessageCreateEventArgs args)
    {
        if (args.Channel.IsPrivate || args.Guild.Id != settings.MainServerId ||
            args.Channel.Id != settings.HoneypotChannelId)
            return false;

        var author = args.Author;

        if (author.Id == client.CurrentUser.Id || author.IsBot || args.Message.WebhookMessage)
            return false;

        if (await TryFindDiscordMemberAsync(args) is not { } member)
            return false;

        if (member.Permissions.HasPermission(Permissions.BanMembers) ||
            member.Permissions.HasPermission(Permissions.Administrator))
            return false;

        try
        {
            await args.Guild.BanMemberAsync(
                args.Message.Author.Id,
                delete_message_days: 1,
                reason: "Automated honeypot ban. Reason: SPAM/SCAM.");

            client.Logger.LogInformation(
                "Honeypot ban: {User} ({Id}), message: {Content}, attachments: {Attachments}",
                author.Username, author.Id, args.Message.Content, string.Join(',', args.Message.Attachments.Select(a => a.Url)));
        }
        catch (UnauthorizedException)
        {
            client.Logger.LogWarning("Honeypot ban failed for {Id}: unsufficient permissions", author.Id);
        }
        catch (Exception ex)
        {
            client.Logger.LogError(ex, "Honeypot ban failed {Id}: check exception for details", author.Id);
        }

        return true; // Consider it handled regardless of ban outcome because honeypot was matched

    }

    private static async Task<DiscordMember?> TryFindDiscordMemberAsync(MessageCreateEventArgs args)
    {
        try
        {
            return args.Author as DiscordMember ?? await args.Guild.GetMemberAsync(args.Author.Id);
        }
        catch (NotFoundException)
        {
            return null;
        }
    }
}