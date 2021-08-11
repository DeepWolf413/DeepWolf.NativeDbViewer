using Newtonsoft.Json;

namespace DeepWolf.NativeDbViewer.Models
{
    [JsonObject]
    public class NativeNamespace
    {
        [JsonConstructor]
        public NativeNamespace(string name, Native[] natives)
        {
            Name = name;
            Natives = natives;
        }

        [JsonProperty]
        public string Name { get; }

        [JsonProperty]
        public Native[] Natives { get; }
    }
}
