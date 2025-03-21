using GorillaInfoWatch.Models;
using GorillaNetworking;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GorillaInfoWatch.Tools
{
    public class Metadata : MonoBehaviour
    {
        private static string BasePath => Path.Combine(Application.persistentDataPath, "GorillaInfoWatchData.txt");

        private static readonly Dictionary<string, object> _sessionData = [];
        private static Dictionary<string, object> _storedData = [];

        public void Awake()
        {
            if (File.Exists(BasePath))
            {
                _storedData = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(BasePath));
            }
            else
            {
                File.WriteAllText(BasePath, JsonConvert.SerializeObject(_storedData));
            }
        }

        public static void AddItem(string key, object value, DataType dataType = DataType.Session)
        {
            Dictionary<string, object> dictionary = dataType == DataType.Session ? _sessionData : _storedData;
            dictionary.AddOrUpdate(key, value);

            if (dataType == DataType.Stored)
            {
                File.WriteAllText(BasePath, JsonConvert.SerializeObject(_storedData));
            }
        }

        public static T GetItem<T>(string key, T defaultValue, DataType dataType = DataType.Session)
        {
            Dictionary<string, object> dictionary = dataType == DataType.Session ? _sessionData : _storedData;

            if (dictionary.TryGetValue(key, out object value))
            {
                return (T)value;
            }

            return defaultValue;
        }

        public static void RemoveItem(string key, DataType dataType = DataType.Session)
        {
            Dictionary<string, object> dictionary = dataType == DataType.Session ? _sessionData : _storedData;

            if (dictionary.ContainsKey(key))
            {
                dictionary.Remove(key);

                if (dataType == DataType.Stored)
                {
                    File.WriteAllText(BasePath, JsonConvert.SerializeObject(_storedData));
                }
            }
        }
    }
}
