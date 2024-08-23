using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using Castle.Core.Internal;
using DSharpPlus;
using DSharpPlus.CommandsNext;
using DSharpPlus.Entities;
using DSharpPlus.Interactivity.Extensions;
using TkokDiscordBot.Analysis;
using TkokDiscordBot.Core.Commands.Dto;
using TkokDiscordBot.Formatters;
using TkokDiscordBot.Helpers;
using TkokDiscordBot.Search;

namespace TkokDiscordBot.Core.CommandsNext;

/// <summary>
/// Base class to be implemented by Commands that can run Search Wizard to find items.
/// </summary>
public abstract class SearchWizardCommandBase : BaseCommandModule
{
    public const string IdPrefix = "search-wizard-";

    private readonly IItemAnalysisService _analysisService;
    private readonly IFullTextSearchService _fullTextSearch;
    private readonly DiscordPageGenerator _pageGenerator = new();

    protected SearchWizardCommandBase(IItemAnalysisService analysisService, IFullTextSearchService fullTextSearch)
    {
        _analysisService = analysisService;
        _fullTextSearch = fullTextSearch;
    }

    /// <summary>
    /// Starts a Search Wizard, which allows users to specify search criteria in an interactive way.
    /// Then, performs a search query and displays the results.
    /// </summary>
    /// <param name="context"></param>
    /// <returns></returns>
    public Task RunSearchWizardAndRespondAsync(CommandContext context)
    {
        _ = Task.Run(async () =>
        {
            var query = await RunSearchWizardAsync(context);

            if (query is null)
            {
                return;
            }

            await SearchAndRespondAsync(context, query, true);
        });

        return Task.CompletedTask;
    }

    /// <summary>
    /// Starts a Search Wizard, which allows users to specify search criteria in an interactive way.
    /// </summary>
    /// <param name="context"></param>
    /// <returns>Lucene-compatible search query or null (if user didn't submit the criteria and command has timed out).</returns>
    protected async Task<string> RunSearchWizardAsync(CommandContext context)
    {
        // Prepare custom bosses list because Discord allows only up to 25 options in dropdown list.
        // Therefore, merge `Broodmother` and `Narith` into a single group.
        var bossesList = _analysisService.AvailableBosses.Except(new[] { "Broodmother", "Narith" }).ToList();
        bossesList.Insert(0, "Broodmother & Narith");

        var filters = new List<SearchWizardFilter>
        {
            new(SearchWizardInputType.Class, _analysisService.AvailableClasses),
            new(SearchWizardInputType.Slot, _analysisService.ItemSlots),
            new(SearchWizardInputType.Type, _analysisService.ItemTypes),
            new(SearchWizardInputType.Boss, bossesList,
                new Dictionary<string, string> { { "Broodmother & Narith", "(Broodmother OR Narith)" } }),
        };
        var submitButton = new DiscordButtonComponent(ButtonStyle.Primary, "search-wizard-submit", "Search!");

        // Build a message (can contain at most 1 Select or 5 Buttons per row) and can have 5 rows at most.
        // We use 4 Select components as a filters for future search and a single Button to submit the result.
        var builder = new DiscordMessageBuilder()
            .WithContent($" {DiscordServerEmojis.Bot} Welcome to the Search Wizard! Select one or more options and press **Search**.");

        foreach (var filter in filters)
        {
            builder.AddComponents(new DiscordSelectComponent(filter.Id, $"Select {filter.Type}", filter.Options, false, 0, filter.Options.Count));
        }

        builder.AddComponents(submitButton);

        var message = await builder.SendAsync(context.Channel);


        // Handle interactions with dropdown components
        var interactivity = context.Client.GetInteractivity();
        using var cts = new CancellationTokenSource();

        var dropdownChangesHandlerTask = interactivity.WaitForSelectAsync(message, args =>
        {
            var filter = filters.Single(f => f.Id == args.Id);
            filter.Select(args.Values);

            return false;
        }, cts.Token);

        // Wait for user to click on `Search!` button (or interactivity timeout).
        var result = await interactivity.WaitForButtonAsync(message, new List<DiscordButtonComponent> { submitButton }, cts.Token);

        // Stop handling value changes in dropdowns & delete the original message
        cts.Cancel();
        await dropdownChangesHandlerTask;
        await message.DeleteAsync();

        if (result.TimedOut)
        {
            return null;
        }

        // Build the query
        var queryGroups = filters.Where(f => !f.SelectedOptions.IsNullOrEmpty())
            .Select(f => f.BuildSearchTerm())
            .ToList();

        if (queryGroups.IsNullOrEmpty())
        {
            await context.RespondAsync($" {DiscordServerEmojis.Think} You need to select at least one filter.");
            return null;
        }

        // Join terms of same kind with `OR`. Then join different categories with `AND`.
        var finalQuery = queryGroups.Count > 1
            ? string.Join(" AND ", queryGroups)
            : queryGroups.Single();

        return finalQuery;
    }

    /// <summary>
    /// Takes a Lucene-compatible query string as an input and
    /// replies to the user with a Markdown-formatted table of found items.
    /// </summary>
    /// <param name="context"></param>
    /// <param name="query"></param>
    /// <param name="displayQuery"></param>
    /// <returns></returns>
    protected async Task SearchAndRespondAsync(CommandContext context, string query, bool displayQuery)
    {
        var items = _fullTextSearch.Search(query);

        if (!items.Any())
        {
            var hasNeckOrRingFilter = Regex.IsMatch(query, @"slot:.*?(Neck|Ring)|type:.*?Accessory", RegexOptions.IgnoreCase);
            var hasClassFilter = Regex.IsMatch(query, @"class:", RegexOptions.IgnoreCase);

            var errorResponseBuilder = new StringBuilder($" {DiscordServerEmojis.MonkaS} There are no matching items");

            if (displayQuery)
            {
                errorResponseBuilder.Append($" (query: `{query}`).");
            }

            if (hasNeckOrRingFilter && hasClassFilter)
            {
                errorResponseBuilder.AppendLine(
                    "\r\n :warning: **Warning**: Remove `Class` filter to search for `Accessories`. Accessories (`Rings` and `Necklaces`) " +
                    "are shared among  all classes and their filter is suppressed by `Class` filter. ");
            }

            await context.RespondAsync(errorResponseBuilder.ToString());
            return;
        }

        var headerBuilder = new StringBuilder($"Found {items.Count} {(items.Count > 1 ? "items" : "item")}.");
        if (displayQuery)
        {
            headerBuilder.Append($" Query: `{query}`.");
        }

        var messagePages = _pageGenerator.ToPages(items, headerBuilder.ToString());
        if (messagePages.Count > 1)
        {
            var interactivity = context.Client.GetInteractivity();

            // Awaiting for completion of the interactivity session deadlocks *any* further
            // commands *on a host with single-core CPU*, but works  properly on multi-code CPU even
            // with InteractivityConfiguration.AckPaginationButtons set to `true`.
            //
            // It's a regression after upgrading to DSharpPlus 4.x from 3.x.
            //
            // Therefore, the task is not awaited below. It will be eventually completed
            // in background when interaction is over or on interactivity timeout.

            _ = interactivity.SendPaginatedMessageAsync(context.Channel, context.User, messagePages);
        }
        else
        {
            await context.RespondAsync(messagePages.Single().Content);
        }
    }
}