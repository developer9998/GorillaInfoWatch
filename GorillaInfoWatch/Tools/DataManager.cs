﻿using GorillaInfoWatch.Models;
using GorillaNetworking;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Net.Http;
using UnityEngine;
using HarmonyLib;

namespace GorillaInfoWatch.Tools
{
    public class DataManager
    {
        private static string BasePath => Path.Combine(Application.persistentDataPath, "GorillaInfoWatchData.txt");

        private static readonly Dictionary<string, object> SessionData = new();
        private static Dictionary<string, object> StoredData = new();

        public DataManager()
        {
            if (File.Exists(BasePath))
            {
                StoredData = JsonConvert.DeserializeObject<Dictionary<string, object>>(File.ReadAllText(BasePath));
            }
            else
            {
                File.WriteAllText(BasePath, JsonConvert.SerializeObject(StoredData));
            }

            LoadRecognisedPlayers();
        }

        private async void LoadRecognisedPlayers()
        {
            try
            {
                HttpClient client = new();
                string rawResult = await client.GetStringAsync("https://raw.githubusercontent.com/developer9998/GorillaInfoWatch/main/RecognisedPlayers.txt");
                string base64Result = Encoding.UTF8.GetString(Convert.FromBase64String(rawResult));

                base64Result.Split('\n').Do(playerId => AddItem(string.Concat(playerId, "rec"), true));
                Logging.Info("Collected list of recognised players");
            }
            catch (Exception exception)
            {
                Logging.Error(string.Concat("LoadRecognisedPlayers threw an exception: ", exception.ToString()));
            }
        }

        public static void AddItem(string key, object value, DataType dataType = DataType.Session)
        {
            Dictionary<string, object> dictionary = dataType == DataType.Session ? SessionData : StoredData;
            dictionary.AddOrUpdate(key, value);

            if (dataType == DataType.Stored)
            {
                File.WriteAllText(BasePath, JsonConvert.SerializeObject(StoredData));
            }
        }

        public static T GetItem<T>(string key, T defaultValue, DataType dataType = DataType.Session)
        {
            Dictionary<string, object> dictionary = dataType == DataType.Session ? SessionData : StoredData;

            if (dictionary.TryGetValue(key, out object value))
            {
                return (T)value;
            }

            return defaultValue;
        }

        public static void RemoveItem(string key, DataType dataType = DataType.Session)
        {
            Dictionary<string, object> dictionary = dataType == DataType.Session ? SessionData : StoredData;

            if (dictionary.ContainsKey(key))
            {
                dictionary.Remove(key);

                if (dataType == DataType.Stored)
                {
                    File.WriteAllText(BasePath, JsonConvert.SerializeObject(StoredData));
                }
            }
        }
    }
}
