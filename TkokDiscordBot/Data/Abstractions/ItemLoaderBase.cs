using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using TkokDiscordBot.Entities;
using TkokDiscordBot.Extensions;

namespace TkokDiscordBot.Data.Abstractions
{
    /// <summary>
    /// Items loader base class. Override <see cref="Load"/> with custom implementation.
    /// </summary>
    public abstract class ItemLoaderBase : IItemsLoader
    {
        readonly Regex _itemHeaderRegex = new Regex(@"^(?:\""(.*?)\"")(?:, \""(.*?)\"")?(?:, SLOT (\b[A-Z]+))?, TYPE (\b[A-Z\s]+)(?:, LEVEL (\d+)(?: \(\d+\))?, QUALITY (\b[A-Z-]+)\s?)?$", RegexOptions.Compiled);

        protected IEnumerable<Item> ParseLines(string[] itemSource)
        {
            var dbSet = new List<Item>();
            var obtainableFrom = string.Empty;
            var i = 0;
            Item currentItem = null;
            while (itemSource.Length > i)
            {
                var currentLine = itemSource[i];

                if (Regex.IsMatch(currentLine, @"^//={81}$")) //new boss/section
                {
                    var nextLine = itemSource[i + 1];
                    obtainableFrom = Regex.Match(nextLine, "^//(.*)$").Result("$1").Trim();
                }
                else if (currentLine.StartsWith("\"")) //new item header
                {
                    var itemRegex = _itemHeaderRegex.Match(currentLine);
                    if (itemRegex.Success)
                    {
                        currentItem = new Item
                        {
                            ObtainableFrom = obtainableFrom,
                            Name = itemRegex.Result("$1"),
                            Description = itemRegex.Result("$2"),
                            Slot = itemRegex.Result("$3").Trim().ToSentenceCase(),
                            Type = itemRegex.Result("$4").Trim().ToSentenceCase()
                        };

                        var lvl = itemRegex.Result("$5").Trim();
                        if (!string.IsNullOrEmpty(lvl))
                            currentItem.Level = short.Parse(lvl);
                        currentItem.Quality = itemRegex.Result("$6").Trim().ToSentenceCase();
                    }
                }
                else if (currentItem != null)
                {
                    if (currentLine.StartsWith("SPECIAL")) //SPECIAL property
                    {
                        var offset = 1;
                        var specialText = string.Empty;
                        while (itemSource.Length > i + offset && !string.IsNullOrWhiteSpace(itemSource[i + offset]))
                        {
                            specialText += itemSource[i + offset].Trim() + "\r\n";
                            offset++;
                        }

                        currentItem.Special = specialText;
                        i += offset - 1;
                    }
                    else if (currentLine.StartsWith("IMAGE")) //IMAGE property
                    {
                        var cursorSplit = currentLine.Split(',');
                        var image = cursorSplit[1].Trim();
                        currentItem.Icon = image;
                    }
                    else if (currentLine.StartsWith("CLASS RESTRICTION")) //CLASS RESTRICTION property
                    {
                        var cursorSplit = currentLine.Split(',');
                        if (cursorSplit.Length >= 2)
                        {
                            currentItem.ClassRestriction = cursorSplit[1].Trim();
                        }
                    }
                    else if (Regex.IsMatch(currentLine, @"^\b[A-Z\s]+")) //line starts with other BOLD ITEM PROPERTY
                    {
                        var cursorSplit = currentLine.Split(',');
                        var key = cursorSplit[0].ToSentenceCase();
                        var value = cursorSplit[1];

                        if (double.TryParse(value, out var decimalValue))
                        {
                            currentItem.Properties.Add(key, decimalValue);
                        }
                    }
                    else if (string.IsNullOrWhiteSpace(currentLine)) //save current item
                    {
                        dbSet.Add(currentItem);
                        currentItem = null;
                    }
                }
                i++;
            }

            return dbSet;
        }

        public abstract Task<IEnumerable<Item>> Load();
    }
}