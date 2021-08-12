using System.Text;
using Newtonsoft.Json;

namespace DeepWolf.NativeDbViewer.Models
{
    [JsonObject]
    public class Native
    {
        [JsonConstructor]
        public Native(string namespaceName, string name, string comment, NativeParam[] parameters, string returnType, string build)
        {
            Namespace = namespaceName;
            Name = name;
            Comment = comment;
            Parameters = parameters;
            ReturnType = returnType;
            Build = build;
            Hash = string.Empty;
            ScriptUsage = string.Empty;
        }

        public string Namespace { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("comment")]
        public string Comment { get; set; }

        [JsonProperty("params")]
        public NativeParam[] Parameters { get; }

        [JsonProperty("return_type")]
        public string ReturnType { get; set; }

        [JsonProperty("build")]
        public string Build { get; set; }

        public string Hash { get; set; }

        public string ScriptUsage { get; set; }

        public override string ToString()
        {
            var parameters = Parameters;
            StringBuilder builder = new StringBuilder();
            foreach (var nativeParam in parameters)
            { builder.Append($"{nativeParam.ToString()}, "); }

            string paramsString = builder.ToString().TrimEnd().TrimEnd(',');
            return $"{ReturnType} {Name}({paramsString})";
        }
    }
}
