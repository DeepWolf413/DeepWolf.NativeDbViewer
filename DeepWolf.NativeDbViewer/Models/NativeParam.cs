using Newtonsoft.Json;

namespace DeepWolf.NativeDbViewer.Models
{
    [JsonObject]
    public class NativeParam
    {
        [JsonConstructor]
        public NativeParam(string type, string name)
        {
            Type = type;
            Name = name;
        }

        [JsonProperty("type")]
        public string Type { get; }

        [JsonProperty("name")]
        public string Name { get; }

        public override string ToString() => $"{Type} {Name}";
    }
}
