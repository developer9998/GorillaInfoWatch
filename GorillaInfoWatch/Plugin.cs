using BepInEx;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Tools;
using HarmonyLib;
using System.Reflection;
using UnityEngine;

namespace GorillaInfoWatch
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    [BepInDependency("net.rusjj.gorillafriends"), BepInDependency("org.legoandmars.gorillatag.utilla")]
    internal class Plugin : BaseUnityPlugin
    {
        internal static Logging Log;
        internal static new Configuration Config;

        public void Awake()
        {
            Log = new Logging(Logger);
            Config = new Configuration(base.Config);

            Harmony.CreateAndPatchAll(Assembly.GetExecutingAssembly(), Constants.GUID);
            GorillaTagger.OnPlayerSpawned(() => DontDestroyOnLoad(new GameObject($"{Constants.Name} {Constants.Version}", typeof(DataManager), typeof(SignificanceManager), typeof(MediaManager), typeof(ShortcutHandler), typeof(Main), typeof(NetworkManager))));
        }
    }
}
