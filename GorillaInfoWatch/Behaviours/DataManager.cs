using BepInEx;
using GorillaInfoWatch.Models.Enumerations;
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

        private static string dataLocation;

        private static Dictionary<DataLocation, Dictionary<string, object>> dataPerTypeDict;

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

            dataPerTypeDict = Enum.GetValues(typeof(DataLocation)).Cast<DataLocation>().ToDictionary(type => type, type => new Dictionary<string, object>());

            dataLocation = Path.Combine(Application.persistentDataPath, $"InfoWatch.json");

            ReadPersistentData();
        }

        public void SetItem(string key, object value, DataLocation dataType = DataLocation.Persistent)
        {
            EnsureDataCollection(dataType);
            Dictionary<string, object> dictionary = dataPerTypeDict[dataType];

            if (dictionary.ContainsKey(key)) dictionary[key] = value;
            else dictionary.Add(key, value);

            if (dataType == DataLocation.Persistent) WritePersistentData();
        }

        public void RemoveItem(string key, DataLocation dataType = DataLocation.Persistent)
        {
            EnsureDataCollection(dataType);
            Dictionary<string, object> dictionary = dataPerTypeDict[dataType];

            if (dictionary.ContainsKey(key))
            {
                dictionary.Remove(key);
                if (dataType == DataLocation.Persistent) WritePersistentData();
            }
        }

        public T GetItem<T>(string key, DataLocation dataType = DataLocation.Persistent, T defaultValue = default, bool setDefaultValue = true)
        {
            EnsureDataCollection(dataType);
            Dictionary<string, object> dictionary = dataPerTypeDict[dataType];

            if (dictionary.TryGetValue(key, out object value))
            {
                if (value is T item) return item;

                TypeCode typeCode = Type.GetTypeCode(typeof(T));

                if (typeCode != TypeCode.Int64 && value is long deserializedLong)
                {
                    switch (typeCode)
                    {
                        case TypeCode.Int16:
                            short int16 = Convert.ToInt16(deserializedLong);
                            dictionary[key] = int16;
                            return (T)(object)int16;
                        case TypeCode.Int32:
                            int int32 = Convert.ToInt32(deserializedLong);
                            dictionary[key] = int32;
                            return (T)(object)int32;
                    }
                }
            }

            if (setDefaultValue) SetItem(key, defaultValue);
            return defaultValue;
        }

        public void ReadPersistentData()
        {
            EnsureDataCollection(DataLocation.Persistent);

            if (File.Exists(dataLocation)) dataPerTypeDict[DataLocation.Persistent] = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(dataLocation));
            else WritePersistentData();
        }

        public void WritePersistentData()
        {
            EnsureDataCollection(DataLocation.Persistent);

            string serialized = JsonConvert.SerializeObject(dataPerTypeDict[DataLocation.Persistent], serializeSettings);

            ThreadingHelper.Instance.StartAsyncInvoke(() =>
            {
                File.WriteAllText(dataLocation, serialized);
                return null;
            });
        }

        public void EnsureDataCollection(DataLocation dataType)
        {
            if (dataPerTypeDict.ContainsKey(dataType)) return;
            dataPerTypeDict.Add(dataType, []);
        }
    }
}