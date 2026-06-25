using DSharpPlus;
using DSharpPlus.CommandsNext.Attributes;
using DSharpPlus.Entities;
using DSharpPlus.EventArgs;
using DSharpPlus.Exceptions;
using JetBrains.Annotations;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using TkokDiscordBot.Configuration;
using TkokDiscordBot.Core.Commands.Abstractions;
using TkokDiscordBot.Enums;

namespace TkokDiscordBot.Core.Commands;

/// <summary>
/// Detects and bans spambots that post the same pair of image attachments (with no text)
/// across multiple channels within a short time window.
/// </summary>
[Hidden]
[UsedImplicitly]
public class SpamBotDetectionCommand(ISettings settings) : IBotCommand
{
    private static readonly TimeSpan Window = TimeSpan.FromMinutes(5);
    private static readonly TimeSpan MaxAccountAge = TimeSpan.FromDays(30);
    private static readonly TimeSpan MaxGuildMemberAge = TimeSpan.FromDays(14);
    private static readonly TimeSpan SweepInterval = TimeSpan.FromMinutes(5);
    private const int MinRequiredAttachments = 2;
    private const int MinDistinctChannels = 2;

    private readonly ConcurrentDictionary<ulong, TrackedUser> _cache = new();
    private long _lastSweepTicks = DateTimeOffset.UtcNow.Ticks;

    public CommandPriority Priority => CommandPriority.Medium;

    public async Task<bool> HandleAsync(DiscordClient client, MessageCreateEventArgs args)
    {
        if (args.Channel.IsPrivate || args.Guild.Id != settings.MainServerId)
            return false;

        var author = args.Author;
        if (author.Id == client.CurrentUser.Id || author.IsBot || args.Message.WebhookMessage)
            return false;

        if (!string.IsNullOrWhiteSpace(args.Message.Content))
            return false;

        if (args.Message.Attachments.Count < MinRequiredAttachments ||
            !args.Message.Attachments.All(IsImage))
            return false;

        var now = DateTimeOffset.UtcNow;
        EvictExpiredEntries(now);

        var signature = BuildSignature(args.Message.Attachments);

        // Cross-channel posting check
        if (!RegisterAndCheck(author.Id, signature, args.Channel.Id, now))
            return false;

        if (await TryFindDiscordMemberAsync(args) is not { } member)
            return false;

        if (member.Permissions.HasPermission(Permissions.BanMembers) ||
            member.Permissions.HasPermission(Permissions.Administrator))
            return false;

        if (now - member.CreationTimestamp >= MaxAccountAge &&
            now - member.JoinedAt >= MaxGuildMemberAge)
        {
            client.Logger.LogInformation(
                "Cross-channel image spam matched for {User} ({Id}) but account is protected by account age (AccountAge: {AccountAge}d, GuidMember: {GuidMember}).",
                author.Username, author.Id, (now - member.CreationTimestamp).TotalDays, (now - member.JoinedAt).TotalDays);
            return false;
        }

        try
        {
            await args.Guild.BanMemberAsync(
                author.Id,
                delete_message_days: 1,
                reason: "Automated honeypot ban. Reason: SPAM/SCAM.");

            client.Logger.LogInformation(
                "Anti-spam ban: {User} ({Id}), channels: {Channels}, attachments: {Attachments}",
                author.Username, author.Id,
                string.Join(',', GetChannels(author.Id)),
                string.Join(',', args.Message.Attachments.Select(a => a.Url)));
        }
        catch (UnauthorizedException)
        {
            client.Logger.LogWarning("Anti-spam ban failed for {Id}: insufficient permissions", author.Id);
        }
        catch (Exception ex)
        {
            client.Logger.LogError(ex, "Anti-spam ban failed for {Id}: see exception for details", author.Id);
        }

        return true;
    }

    private bool RegisterAndCheck(ulong userId, string signature, ulong channelId, DateTimeOffset now)
    {
        var entry = _cache.GetOrAdd(userId, _ => new TrackedUser());
        lock (entry)
        {
            // Reset the window if the image pair changed or the window expired.
            if (entry.Signature != signature || now - entry.FirstSeen > Window)
            {
                entry.Signature = signature;
                entry.FirstSeen = now;
                entry.Channels.Clear();
                entry.Resolved = false;
            }

            entry.Channels.Add(channelId);

            // Act (resolve) only once per window
            if (!entry.Resolved && entry.Channels.Count >= MinDistinctChannels)
            {
                entry.Resolved = true;
                return true;
            }

            return false;
        }
    }

    private HashSet<ulong> GetChannels(ulong userId) => _cache.TryGetValue(userId, out var e) ? e.Channels : [];

    private void EvictExpiredEntries(DateTimeOffset now)
    {
        if (!TryBeginSweep(now))
            return;

        foreach (var (key, entry) in _cache)
        {
            bool expired;
            lock (entry.Gate)
                expired = now - entry.FirstSeen > Window;

            if (expired)
                _cache.TryRemove(key, out _);
        }
    }

    private bool TryBeginSweep(DateTimeOffset now)
    {
        var last = Interlocked.Read(ref _lastSweepTicks);

        if (now.Ticks - last < SweepInterval.Ticks)
            return false;

        // Returns true only for the caller that wins the window.
        return Interlocked.CompareExchange(ref _lastSweepTicks, now.Ticks, last) == last;
    }

    private static string BuildSignature(IReadOnlyList<DiscordAttachment> attachments)
        => string.Join("|", attachments.Select(a => $"{a.Width}x{a.Height}").OrderBy(s => s, StringComparer.Ordinal));

    private static bool IsImage(DiscordAttachment a) => a is { Width: > 0, Height: > 0 };

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

    private sealed class TrackedUser
    {
        public Lock Gate { get; } = new();
        public string? Signature { get; set; }
        public DateTimeOffset FirstSeen { get; set; }
        public HashSet<ulong> Channels { get; } = [];
        public bool Resolved { get; set; }
    }
}