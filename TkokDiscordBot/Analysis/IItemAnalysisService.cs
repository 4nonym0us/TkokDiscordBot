using System.Collections.Generic;

namespace TkokDiscordBot.Analysis;

/// <summary>
/// Provides an information about the map like in-game character classes, bosses, item properties.
/// </summary>
public interface IItemAnalysisService
{
    List<string> AvailableClasses { get; }

    List<string> ItemSlots { get; }

    List<string> ItemTypes { get; }

    List<string> ItemSources { get; }

    List<string> QualityLevels { get; }

    List<string> AvailableBosses { get; }
}