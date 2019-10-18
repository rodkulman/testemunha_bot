using System;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Rodkulman.TestemunhaBot
{
    public static class DB
    {
        private static List<long> chatIds;
        private static Dictionary<string, string> keys;

        public static IEnumerable<long> Chats { get { return chatIds; } }

        public static DateTime BreakfastMessageLastSent { get; internal set; } = DateTime.Today.AddDays(-1);
        public static DateTime LunchTimeMessageSent { get; internal set; } = DateTime.Today.AddDays(-1);

        public static void Load()
        {
            chatIds = new List<long>();
            keys = new Dictionary<string, string>();
            
            using (var stream = File.OpenRead("db/keys.json"))
            using (var textReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                var raw = JObject.Load(jsonReader);

                foreach (JProperty prop in raw.Properties())
                {
                    keys[prop.Name] = prop.Value.Value<string>();
                }
            }

            using (var stream = File.OpenRead("db/chats.json"))
            using (var textReader = new StreamReader(stream))
            using (var jsonReader = new JsonTextReader(textReader))
            {
                var raw = JArray.Load(jsonReader);

                foreach (var id in raw.Values<long>())
                {
                    chatIds.Add(id);
                }
            }
        }

        public static void AddChat(long id)
        {
            if (!chatIds.Contains(id)) { chatIds.Add(id); }

            SaveChats();
        }

        public static void RemoveChat(long id)
        {
            if (chatIds.Contains(id)) { chatIds.Remove(id); }

            SaveChats();
        }

        private static void SaveChats()
        {
            using (var stream = File.OpenWrite("db/chats.json"))
            using (var textWriter = new StreamWriter(stream))
            using (var jsonWriter = new JsonTextWriter(textWriter))
            {
                JArray.FromObject(chatIds).WriteTo(jsonWriter);
            }
        }

        public static string GetKey(string keyName)
        {
            return keys[keyName];
        }
    }
}