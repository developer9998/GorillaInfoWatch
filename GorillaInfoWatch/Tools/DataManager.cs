using GorillaNetworking;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace GorillaInfoWatch.Tools
{
    public class DataManager
    {
        private static string BasePath => Path.Combine(Application.persistentDataPath, "GorillaInfoWatchData.txt");

        private static Dictionary<string, object> Data = new();

        public DataManager()
        {
            if (File.Exists(BasePath))
            {
                Data = JsonUtility.FromJson<Dictionary<string, object>>(File.ReadAllText(BasePath));
            }
            else
            {
                string contents = JsonUtility.ToJson(Data, true);
                File.WriteAllText(BasePath, contents);
            }
        }

        public static void AddItem(string key, object value)
        {
            Data.AddOrUpdate(key, value);

            string contents = JsonUtility.ToJson(Data, true);
            File.WriteAllText(BasePath, contents);
        }

        public static T GetItem<T>(string key)
        {
            if (Data.TryGetValue(key, out object value))
            {
                return (T)value;
            }
            else
            {
                return default;
            }
        }
    }
}
