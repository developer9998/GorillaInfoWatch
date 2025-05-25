using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Tools;
using HarmonyLib;
using UnityEngine;

namespace GorillaInfoWatch
{
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource PluginLogger;

        public static ConfigFile PluginConfig;

        public void Awake()
        {
            PluginLogger = Logger;
            PluginConfig = Config;

            Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, Constants.Guid);
            GorillaTagger.OnPlayerSpawned(() => new GameObject(Constants.Name, typeof(Main), typeof(NetworkHandler), typeof(DataHandler)));
        }
    }
}
