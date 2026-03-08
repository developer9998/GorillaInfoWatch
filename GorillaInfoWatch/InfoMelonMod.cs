using GorillaInfoWatch;
using GorillaInfoWatch.Behaviours;
using GorillaInfoWatch.Behaviours.Networking;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using GorillaLibrary;
using MelonLoader;
using UnityEngine;

[assembly: MelonInfo(typeof(InfoMelonMod), "GorillaInfoWatch", "1.1.4", "dev9998")]
[assembly: MelonGame("Another Axiom", "Gorilla Tag")]
[assembly: MelonAdditionalDependencies("GorillaLibrary")]
[assembly: MelonOptionalDependencies("GorillaFriends")]

namespace GorillaInfoWatch;

internal class InfoMelonMod : GorillaMod
{
    internal static Logging Log;
    internal static Configuration Config;

    public override void OnInitializeMelon()
    {
        Log = new Logging(LoggerInstance);

        Config = new Configuration(this);

        GorillaLibrary.Events.Game.OnGameInitialized.Subscribe(Initialize);
        GorillaLibrary.Events.Rig.OnRigAdded.Subscribe(RigAdded);
        GorillaLibrary.Events.Rig.OnRigRemoved.Subscribe(RigRemoved);
    }

    public override void OnLateInitializeMelon()
    {
        FriendUtility.ScanPlugins(MelonBase.RegisteredMelons);
    }

    public void Initialize()
    {
        GameObject root = new($"{Info.Name} - {Info.Version}", typeof(DataManager), typeof(SignificanceManager), typeof(MediaManager), typeof(ShortcutHandler), typeof(StatisticsManager), typeof(WatchManager), typeof(NetworkManager));
        Object.DontDestroyOnLoad(root);
    }

    public void RigAdded(VRRig rig, NetPlayer player)
    {
        if (rig.GetComponent<NetworkedPlayer>()) return;

        NetworkedPlayer component = rig.gameObject.AddComponent<NetworkedPlayer>();
        component.Rig = rig;
        component.Player = player;
    }

    public void RigRemoved(VRRig rig)
    {
        if (rig.TryGetComponent(out NetworkedPlayer component))
        {
            Object.Destroy(component);
        }
    }
}
