using BepInEx;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Tools;
using HarmonyLib;
using UnityEngine;

namespace GorillaInfoWatch
{
    [BepInPlugin(Constants.GUID, Constants.Name, Constants.Version)]
    public class Plugin : BaseUnityPlugin
    {
        public void Awake()
        {
            new Logging(Logger);
            new Configuration(Config);

            Harmony.CreateAndPatchAll(typeof(Plugin).Assembly, Constants.GUID);
            GorillaTagger.OnPlayerSpawned(() => new GameObject(Constants.Name, typeof(Main), typeof(NetworkHandler), typeof(DataHandler)));
        }
    }
}
