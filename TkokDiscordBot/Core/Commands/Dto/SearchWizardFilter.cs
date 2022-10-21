using System.Collections.Generic;
using System.Linq;
using DSharpPlus.Entities;
using TkokDiscordBot.Core.CommandsNext;
using TkokDiscordBot.Extensions;

namespace TkokDiscordBot.Core.Commands.Dto;

public class SearchWizardFilter
{
    public const string IdPrefix = SearchWizardCommandBase.IdPrefix + "dropdown-";

    private readonly List<string> _selectedOptions = new();

    public string Id { get; }

    public SearchWizardInputType Type { get; }

    public List<DiscordSelectComponentOption> Options { get; }

    public IReadOnlyCollection<string> SelectedOptions => _selectedOptions;

    /// <summary>
    /// Creates a new instance of <see cref="SearchWizardFilter"/>.
    /// </summary>
    /// <param name="type">Type of a filter.</param>
    /// <param name="labels">List of strings to use as Labels/Values. Values are converted to Lucene terms using <see cref="StringExtensions.AsLuceneTerm"/></param>
    /// <param name="customValuesDict">A dictionary of Lucene terms to use for specific labels.</param>
    public SearchWizardFilter(SearchWizardInputType type, IEnumerable<string> labels, IReadOnlyDictionary<string, string> customValuesDict = null)
    {
        Id = IdPrefix + type.ToString().ToLower();
        Type = type;
        Options = labels.Select(label =>
        {
            var value = customValuesDict != null && customValuesDict.ContainsKey(label)
                ? customValuesDict[label]
                : label.AsLuceneTerm();
            var option = new DiscordSelectComponentOption(label, value);
            return option;
        }).ToList();
    }

    public string BuildSearchTerm()
    {
        if (!_selectedOptions.Any())
        {
            return null;
        }

        // Convert filter type to Lucene index field name
        var field = Type switch
        {
            SearchWizardInputType.Boss => "source",
            SearchWizardInputType.Class => "class",
            _ => Type.ToString().ToCamelCase()
        };

        var searchTerm = string.Join(" OR ", _selectedOptions);
        return _selectedOptions.Count > 1
            ? $"{field}:({searchTerm})"
            : $"{field}:{searchTerm}";
    }

    public void Select(IEnumerable<string> values)
    {
        _selectedOptions.Clear();
        _selectedOptions.AddRange(values);
    }
}