using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Models;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GorillaInfoWatch.Tools
{
    public class DataHandler : Singleton<DataHandler>
    {
        private static string DataPath => Path.Combine(Application.persistentDataPath, $"{Constants.Name}.txt");
        private readonly Dictionary<string, object> session_data = [];
        private Dictionary<string, object> stored_data = [];

        public override void Initialize()
        {
            if (File.Exists(DataPath))
            {
                stored_data = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(DataPath));
            }
            else
            {
                File.WriteAllText(DataPath, JsonConvert.SerializeObject(stored_data, Formatting.None));
            }
        }

        public void AddItem(string key, object value, DataType dataType = DataType.Session)
        {
            Dictionary<string, object> dictionary = dataType == DataType.Session ? session_data : stored_data;
            if (dictionary.ContainsKey(key)) dictionary[key] = value;
            else dictionary.Add(key, value);

            if (dataType == DataType.Stored)
            {
                File.WriteAllText(DataPath, JsonConvert.SerializeObject(stored_data));
            }
        }

        public T GetItem<T>(string key, T defaultValue, DataType dataType = DataType.Session)
        {
            Dictionary<string, object> dictionary = dataType == DataType.Session ? session_data : stored_data;

            if (dictionary.TryGetValue(key, out object value))
            {
                return (T)value;
            }

            return defaultValue;
        }

        public void RemoveItem(string key, DataType dataType = DataType.Session)
        {
            Dictionary<string, object> dictionary = dataType == DataType.Session ? session_data : stored_data;

            if (dictionary.ContainsKey(key))
            {
                dictionary.Remove(key);

                if (dataType == DataType.Stored)
                {
                    File.WriteAllText(DataPath, JsonConvert.SerializeObject(stored_data));
                }
            }
        }
    }
}
