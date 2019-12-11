using System;
using Newtonsoft.Json;

namespace Enpass2HandySafe.EnpassModel
{
    [JsonObject]
    public class Enpass
    {
        [JsonProperty]
        public Folder[] Folders { get; set; }
        [JsonProperty]
        public Item[] Items { get; set; }
    }

    [JsonObject]
    public class Folder
    {
        [JsonProperty]
        public string Icon { get; set; }
        [JsonProperty]
        public string Title { get; set; }
        [JsonProperty]
        public string Uuid { get; set; }
    }

    [JsonObject]
    public class Field
    {
        [JsonProperty]
        public string Label { get; set; }
        [JsonProperty]
        public int Order { get; set; }
        [JsonProperty]
        public int Sensitive { get; set; }
        [JsonProperty]
        public string Type { get; set; }
        [JsonProperty]
        public string Value { get; set; }
    }

    [JsonObject]
    public class Icon
    {
        [JsonProperty]
        public string Fav { get; set; }
        [JsonProperty]
        public string Type { get; set; }
    }

    [JsonObject]
    public class Item
    {
        [JsonProperty]
        public string Title { get; set; }
        [JsonProperty]
        public string Subtitle { get; set; }
        [JsonProperty]
        public string Category { get; set; }
        [JsonProperty]
        public string Note { get; set; }
        [JsonProperty]
        public Icon Icon { get; set; }
        [JsonProperty]
        public string[] Folders { get; set; }
        [JsonProperty]
        public Field[] Fields { get; set; }

    }
}
