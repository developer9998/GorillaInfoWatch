using BepInEx;
using BepInEx.Configuration;
using BepInEx.Logging;
using GorillaInfoWatch.Behaviours;
using HarmonyLib;
using UnityEngine;
using Utilla.Attributes;

namespace GorillaInfoWatch
{
    [ModdedGamemode, BepInDependency("org.legoandmars.gorillatag.utilla"), BepInDependency("dev.auros.bepinex.bepinject")]
    [BepInPlugin(Constants.Guid, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public static ManualLogSource PluginLogger;
        
        public static ConfigFile PluginConfig;

        public static bool InModdedRoom;

        public Plugin()
        {
            PluginLogger = Logger;
            PluginConfig = Config;

            Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, Constants.Guid);
            GorillaTagger.OnPlayerSpawned(() => new GameObject(typeof(Main).FullName).AddComponent<Main>());
        }

        [ModdedGamemodeJoin]
        public void OnModdedJoin() => InModdedRoom = true;

        [ModdedGamemodeLeave]
        public void OnModdedLeave() => InModdedRoom = false;
    }
}
