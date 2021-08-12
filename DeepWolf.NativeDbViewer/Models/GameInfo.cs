using Newtonsoft.Json;

namespace DeepWolf.NativeDbViewer.Models
{
    [JsonObject]
    public class GameInfo
    {
        [JsonConstructor]
        public GameInfo(string nativesLink, string scriptUsagesMapLink)
        {
            NativesLink = nativesLink;
            ScriptUsagesMapLink = scriptUsagesMapLink;
        }

        [JsonProperty("natives-link")]
        public string NativesLink { get; }

        [JsonProperty("script-usages-map-link")]
        public string ScriptUsagesMapLink { get; }
    }
}
