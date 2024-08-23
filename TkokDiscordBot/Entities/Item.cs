using System;
using System.Collections.Generic;
using System.Linq;
using Castle.Core.Internal;

namespace TkokDiscordBot.Entities
{
    [Serializable]
    public class Item
    {
        private short _reforgeLevel;

        public Item()
        {
            Properties = new Dictionary<string, double>();
        }

        public string Name { get; set; }

        public string Description { get; set; }

        public string Type { get; set; }

        public string Slot { get; set; }

        public short Level { get; set; }

        public string Quality { get; set; }

        public string ObtainableFrom { get; set; }

        public string Special { get; set; }

        public string Icon { get; set; }

        public string IconUrl => !Icon.IsNullOrEmpty() ? "https://angelxice.ca/etc/tkok/icons/" + Icon : string.Empty;

        public string ClassRestriction { get; set; }

        public short ReforgeLevel
        {
            get => _reforgeLevel;
            set
            {
                _reforgeLevel = value;
                foreach (var key in Properties.Keys.ToList())
                {
                    Properties[key] = Math.Round(Properties[key] * (1 + value / 30d), 2);
                }
            }
        }

        public bool IsReforged => ReforgeLevel > 0;

        public string ReforgedName => IsReforged ? $"{Name} [+{ReforgeLevel}]" : Name;

        public Dictionary<string, double> Properties { get; set; }
    }
}