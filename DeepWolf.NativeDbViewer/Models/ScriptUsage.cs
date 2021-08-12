using Newtonsoft.Json;

namespace DeepWolf.NativeDbViewer.Models
{
    [JsonObject]
    public class ScriptUsage
    {
        [JsonConstructor]
        public ScriptUsage(string hash, string codeExample)
        {
            Hash = hash;
            CodeExample = codeExample;
        }

        [JsonProperty]
        public string Hash { get; }

        [JsonProperty]
        public string CodeExample { get; }
    }
}