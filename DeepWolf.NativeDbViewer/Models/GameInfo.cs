using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using Newtonsoft.Json;

namespace DeepWolf.NativeDbViewer.Models
{
    [JsonObject]
    public class GameInfo
    {
        public const string GamesNativeInfoFilePath = "games-native-info.json";
        public const string CacheDirectory = "cache";

        [JsonConstructor]
        public GameInfo(string name, string nativesLink, string scriptUsagesMapLink)
        {
            Name = name;
            NativesLink = nativesLink;
            ScriptUsagesMapLink = scriptUsagesMapLink;
        }

        [JsonProperty("name")]
        public string Name { get; }

        [JsonProperty("natives-link")]
        public string NativesLink { get; }

        [JsonProperty("script-usages-map-link")]
        public string ScriptUsagesMapLink { get; }

        public async Task<(bool, Dictionary<string, Dictionary<string, Native>>)> TryGetNativeDatabase()
        {
            return await Task.Run(() =>
            {
                using (var client = new HttpClient())
                using (Stream stream = client.GetStreamAsync(NativesLink).Result)
                using (StreamReader streamReader = new StreamReader(stream))
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (true, serializer.Deserialize<Dictionary<string, Dictionary<string, Native>>>(jsonReader));
                }
            });
        }

        public async Task<(bool, ScriptUsage[])> TryGetScriptUsages()
        {
            if (string.IsNullOrEmpty(ScriptUsagesMapLink))
            { return (false, Array.Empty<ScriptUsage>()); }

            return await Task.Run(() =>
            {
                using (var client = new HttpClient())
                using (Stream stream = client.GetStreamAsync(ScriptUsagesMapLink).Result)
                using (StreamReader streamReader = new StreamReader(stream))
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (true, serializer.Deserialize<ScriptUsage[]>(jsonReader));
                }
            });
        }

        public async Task UpdateCachedNativeDb()
        {
            if (!Directory.Exists(CacheDirectory))
            { Directory.CreateDirectory(CacheDirectory); }

            string nativeDbFilePath = GetNativeDbFilePath();
            string scriptUsagesFilePath = GetScriptUsagesFilePath();

            using (var client = new WebClient())
            {
                var isCachedNativeDbOld = GetAgeOfCachedNativeDb().Days >= 1;
                var isNativeDbCached = IsNativeDbCached();
                if (isCachedNativeDbOld && isNativeDbCached || !isNativeDbCached)
                {
                    File.Delete(nativeDbFilePath);
                    await client.DownloadFileTaskAsync(NativesLink, nativeDbFilePath);
                }

                if (string.IsNullOrEmpty(ScriptUsagesMapLink))
                { return; }

                var isCachedScriptUsagesOld = GetAgeOfCachedScriptUsages().Days >= 1;
                var isScriptUsagesCached = IsScriptUsagesCached();
                if (isCachedScriptUsagesOld && isScriptUsagesCached || !isScriptUsagesCached)
                {
                    File.Delete(scriptUsagesFilePath);
                    await client.DownloadFileTaskAsync(ScriptUsagesMapLink, scriptUsagesFilePath);
                }
            }
        }

        public async Task<(bool, Dictionary<string, Dictionary<string, Native>>)> TryGetNativeDbFromCache()
        {
            string nativeDbFilePath = GetNativeDbFilePath();
            if (!File.Exists(nativeDbFilePath))
            { return (false, null); }

            return await Task.Run(() =>
            {
                using (Stream stream = File.OpenRead(nativeDbFilePath))
                using (StreamReader streamReader = new StreamReader(stream))
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (true, serializer.Deserialize<Dictionary<string, Dictionary<string, Native>>>(jsonReader));
                }
            });
        }

        public async Task<(bool, ScriptUsage[])> TryGetScriptUsagesFromCache()
        {
            string scriptUsagesFilePath = GetScriptUsagesFilePath();
            if (!File.Exists(scriptUsagesFilePath))
            { return (false, null); }

            return await Task.Run(() =>
            {
                using (Stream stream = File.OpenRead(scriptUsagesFilePath))
                using (StreamReader streamReader = new StreamReader(stream))
                using (JsonReader jsonReader = new JsonTextReader(streamReader))
                {
                    JsonSerializer serializer = new JsonSerializer();
                    return (true, serializer.Deserialize<ScriptUsage[]>(jsonReader));
                }
            });
        }

        public bool IsNativeDbCached()
        {
            string nativeDbFilePath = GetNativeDbFilePath();
            return File.Exists(nativeDbFilePath);
        }

        public bool IsScriptUsagesCached()
        {
            string nativeDbFilePath = GetScriptUsagesFilePath();
            return File.Exists(nativeDbFilePath);
        }

        public TimeSpan GetAgeOfCachedNativeDb()
        {
            if (!IsNativeDbCached())
            { return TimeSpan.Zero; }

            var lastModifiedDate = File.GetLastWriteTime(GetNativeDbFilePath());
            return DateTime.Now - lastModifiedDate;
        }

        public TimeSpan GetAgeOfCachedScriptUsages()
        {
            if (!IsScriptUsagesCached())
            { return TimeSpan.Zero; }

            var lastModifiedDate = File.GetLastWriteTime(GetScriptUsagesFilePath());
            return DateTime.Now - lastModifiedDate;
        }

        private string GetNativeDbFilePath() => Path.Combine(CacheDirectory, $"{Name}-nativedb.json");

        private string GetScriptUsagesFilePath() => Path.Combine(CacheDirectory, $"{Name}-scriptusages.json");

        private async Task<(bool, Dictionary<string, Dictionary<string, Native>>)> TryGetNativeDbFromRepo()
        {
            var (hasFoundNativeDb, nativeDb) = await TryGetNativeDatabase();
            if (!hasFoundNativeDb)
            { return (false, null); }

            return (true, nativeDb);
        }

        private async Task<(bool, ScriptUsage[])> TryGetScriptUsagesFromRepo()
        {
            var (hasFoundScriptUsages, scriptUsages) = await TryGetScriptUsages();
            return (hasFoundScriptUsages, scriptUsages);
        }
    }
}
