namespace TkokDiscordBot.Core.Commands.Dto;

public enum SearchWizardInputType
{
    /// <summary>
    /// Character class filter.
    /// </summary>
    Class,

    /// <summary>
    /// Inventory slot filter.
    /// </summary>
    Slot,

    /// <summary>
    /// Item type filter.
    /// </summary>
    Type,

    /// <summary>
    /// Item source filter.
    /// </summary>
    Boss
}