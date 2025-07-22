using GorillaInfoWatch.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours
{
    public class DataManager : MonoBehaviour
    {
        public static DataManager Instance { get; private set; }

        private string dataLocation;

        private Dictionary<DataType, Dictionary<string, object>> dataPerTypeDict;

        private JsonSerializerSettings serializeSettings, deserializeSettings;

        public void Awake()
        {
            if (Instance != null && (bool)Instance && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            var converter = new Vector3Converter();

            serializeSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All,
                CheckAdditionalContent = true,
                Formatting = Formatting.Indented
            };
            serializeSettings.Converters.Add(converter);

            deserializeSettings = new JsonSerializerSettings
            {
                TypeNameHandling = TypeNameHandling.All
            };
            deserializeSettings.Converters.Add(converter);

            dataPerTypeDict = Enum.GetValues(typeof(DataType)).Cast<DataType>().ToDictionary(type => type, type => new Dictionary<string, object>());

            dataLocation = Path.Combine(Application.persistentDataPath, $"{Constants.Name}.json");

            ReadPersistentData();
        }

        public void AddItem(string key, object value, DataType dataType = DataType.Persistent)
        {
            EnsureDataCollection(dataType);
            Dictionary<string, object> dictionary = dataPerTypeDict[dataType];

            if (dictionary.ContainsKey(key)) dictionary[key] = value;
            else dictionary.Add(key, value);

            if (dataType == DataType.Persistent) WritePersistentData();
        }

        public void RemoveItem(string key, DataType dataType = DataType.Persistent)
        {
            EnsureDataCollection(dataType);
            Dictionary<string, object> dictionary = dataPerTypeDict[dataType];

            if (dictionary.ContainsKey(key))
            {
                dictionary.Remove(key);
                if (dataType == DataType.Persistent) WritePersistentData();
            }
        }

        public T GetItem<T>(string key, T defaultValue = default, DataType dataType = DataType.Persistent)
        {
            EnsureDataCollection(dataType);
            Dictionary<string, object> dictionary = dataPerTypeDict[dataType];

            if (dictionary.TryGetValue(key, out object value))
            {
                return (T)value;
            }

            return defaultValue;
        }

        public void ReadPersistentData()
        {
            EnsureDataCollection(DataType.Persistent);

            if (File.Exists(dataLocation)) dataPerTypeDict[DataType.Persistent] = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(dataLocation));
            else WritePersistentData();
        }

        public void WritePersistentData()
        {
            EnsureDataCollection(DataType.Persistent);

            File.WriteAllText(dataLocation, JsonConvert.SerializeObject(dataPerTypeDict[DataType.Persistent], serializeSettings));
        }

        public void EnsureDataCollection(DataType dataType)
        {
            if (dataPerTypeDict.ContainsKey(dataType)) return;
            dataPerTypeDict.Add(dataType, []);
        }
    }
}