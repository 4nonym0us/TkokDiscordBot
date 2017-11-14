using System.Collections.Generic;
using Castle.Core.Internal;

namespace TkokDiscordBot.Entities
{
    public class Item
    {
        public Item()
        {
            Properties = new Dictionary<string, string>();
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

        public Dictionary<string, string> Properties { get; set; }
    }
}