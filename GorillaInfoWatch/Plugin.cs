using BepInEx;
using BepInEx.Bootstrap;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace GorillaInfoWatch
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    [BepInDependency("org.legoandmars.gorillatag.utilla")]
    [BepInDependency("net.rusjj.gorillafriends", BepInDependency.DependencyFlags.SoftDependency)]
    internal class Plugin : BaseUnityPlugin
    {
        internal static Logging Log;
        internal static new Configuration Config;

        public void Awake()
        {
            Log = new Logging(Logger);
            Config = new Configuration(base.Config);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Constants.GUID);
            GorillaTagger.OnPlayerSpawned(() => DontDestroyOnLoad(new GameObject($"{Constants.Name} {Constants.Version}", typeof(DataManager), typeof(SignificanceManager), typeof(MediaManager), typeof(ShortcutHandler), typeof(StatisticsManager), typeof(Main), typeof(NetworkManager))));

            FriendUtility.ScanPlugins(Chainloader.PluginInfos);
        }
    }
}
