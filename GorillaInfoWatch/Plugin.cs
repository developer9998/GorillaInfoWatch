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
    [BepInDependency("net.rusjj.gorillafriends")]
    [BepInDependency("org.legoandmars.gorillatag.utilla")]
    public class Plugin : BaseUnityPlugin
    {
        private Assembly nativeAssembly;

        public void Awake()
        {
            new Logging(Logger);
            new Configuration(Config);

            nativeAssembly = typeof(Plugin).Assembly;
            Harmony.CreateAndPatchAll(nativeAssembly, Constants.GUID);
            GorillaTagger.OnPlayerSpawned(() => new GameObject(Constants.Name, typeof(SignificanceManager), typeof(Main), typeof(NetworkManager), typeof(DataManager)));
        }
    }
}
