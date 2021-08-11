using System;
using DeepWolf.NativeDbViewer.Models;
using Newtonsoft.Json;

namespace DeepWolf.NativeDbViewer.JsonConverters
{
    public class NativeNamespaceConverter : JsonConverter
    {
        public override void WriteJson(JsonWriter writer, object value, JsonSerializer serializer)
        {
            var nativeNamespace = (NativeNamespace) value;
            writer.WriteValue(nativeNamespace.Name);
            writer.WriteValue(nativeNamespace.Natives);
        }

        public override object ReadJson(JsonReader reader, Type objectType, object existingValue, JsonSerializer serializer)
        {
            if (reader.TokenType == JsonToken.StartArray)
            {
                return serializer.Deserialize<NativeNamespace[]>(reader);
            }
            else if(reader.TokenType == JsonToken.StartObject)
            {
                /*var nativeNamespace = serializer.Deserialize<NativeNamespace>(reader);
                return new NativeNamespace[]*/
            }

            return null;
        }

        public override bool CanConvert(Type objectType)
        {
            throw new NotImplementedException();
        }
    }
}
