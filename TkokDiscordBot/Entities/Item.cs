using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using Castle.Core.Internal;
using TkokDiscordBot.Helpers;

namespace TkokDiscordBot.Entities;

/// <summary>
/// Represents an Item from loot table.
/// </summary>
public class Item : ICloneable
{
    private short _reforgeLevel;

    public int Id { get; set; }

    public string Name { get; set; }

    public string Description { get; set; }

    public string Type { get; set; }

    public string Slot { get; set; }

    public short Level { get; set; }

    public string Quality { get; set; }

    public string ObtainableFrom { get; set; }

    public string NormalizedObtainableFrom => Regex.Replace(ObtainableFrom, @"\s*?\[.*?\]\s*?", string.Empty);

    public string Special { get; set; }

    public string Icon { get; set; }

    public string IconUrl => !Icon.IsNullOrEmpty() ? "http://185.10.17.236/icons/" + Icon : string.Empty;

    public string ClassRestriction { get; set; }

    public short ReforgeLevel
    {
        get => _reforgeLevel;
        set
        {
            _reforgeLevel = value;
            ReforgingHelper.ReforgeProperties(this, value);
        }
    }

    public bool IsReforged => ReforgeLevel > 0;

    public string ReforgedName => IsReforged ? $"{Name} [+{ReforgeLevel}]" : Name;

    public Dictionary<string, double> Properties { get; private set; } = new();

    public object Clone()
    {
        var item = (Item)MemberwiseClone();
        item.Properties = new Dictionary<string, double>(item.Properties);
        return item;
    }
}