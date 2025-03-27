using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Tools;
using HarmonyLib;
using UnityEngine;
using Utilla.Attributes;

namespace GorillaInfoWatch
{
    [ModdedGamemode, BepInDependency("org.legoandmars.gorillatag.utilla")]
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource PluginLogger;
        
        public static ConfigFile PluginConfig;

        public static bool InModdedRoom;

        public void Awake()
        {
            PluginLogger = Logger;
            PluginConfig = Config;

            Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, Constants.Guid);
            GorillaTagger.OnPlayerSpawned(() => new GameObject(Constants.Name, typeof(Main), typeof(NetworkHandler), typeof(DataHandler)));
        }

        [ModdedGamemodeJoin]
        public void OnModdedJoin() => InModdedRoom = true;

        [ModdedGamemodeLeave]
        public void OnModdedLeave() => InModdedRoom = false;
    }
}
