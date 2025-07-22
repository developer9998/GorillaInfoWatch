using BepInEx;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Tools;
using HarmonyLib;
using UnityEngine;

namespace GorillaInfoWatch
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    [BepInDependency("net.rusjj.gorillafriends", "1.2.3")]
    [BepInDependency("org.legoandmars.gorillatag.utilla", "1.6.18")]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            new Logging(Logger);
            new Configuration(Config);

            Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, Constants.GUID);
            GorillaTagger.OnPlayerSpawned(() => new GameObject(Constants.Name, typeof(Main), typeof(NetworkManager), typeof(DataManager)));
        }
    }
}
