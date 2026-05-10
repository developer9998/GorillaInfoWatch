using GorillaInfoWatch.Models.Shortcuts;
using GorillaInfoWatch.Tools;
using GorillaNetworking;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;

namespace GorillaInfoWatch.Shortcuts.Rooms;

internal class JoinRandomRoom : Shortcut
{
    public override string Name => "Join Random Room";
    public override string Description => "Joins a random public room under a map that is loaded";

    public override async void Invoke(bool isStateEnabled)
    {
        List<GTZone> activeZones = ZoneManagement.instance.activeZones;

        List<GorillaNetworkJoinTrigger> triggers = [];

        foreach (var (networkZone, joinTrigger) in GorillaComputer.instance.primaryTriggersByZone)
        {
            if (joinTrigger == null) continue;

            if (activeZones.Contains(joinTrigger.zone) && (!NetworkSystem.Instance.InRoom || GorillaComputer.instance.friendJoinCollider == null || GorillaComputer.instance.friendJoinCollider.myAllowedMapsToJoin.Contains(networkZone) || (!NetworkSystem.Instance.SessionIsPrivate && GorillaComputer.instance.GetJoinTriggerFromFullGameModeString(NetworkSystem.Instance.GameModeString) is GorillaNetworkJoinTrigger triggerFromMode && triggerFromMode.myCollider.myAllowedMapsToJoin.Contains(networkZone))))
            {
                triggers.Add(joinTrigger);
                Logging.Info($"+ {joinTrigger.zone} ({string.Join(", ", joinTrigger.myCollider?.myAllowedMapsToJoin)})");
            }
        }

        if (triggers.Count == 0)
        {
            Logging.Warning("No available join triggers found");
            return;
        }

        GorillaNetworkJoinTrigger selectedTrigger = triggers.Count == 1 ? triggers[0] : triggers[Random.Range(0, triggers.Count - 1)];

        Logging.Message($"Join Trigger: {selectedTrigger} ({selectedTrigger.zone.GetName()})");

        if (NetworkSystem.Instance.InRoom)
        {
            Logging.Warning("Disconnecting from current room");

            await NetworkSystem.Instance.ReturnToSinglePlayer();
            await Task.Delay(250);
        }

        selectedTrigger.OnBoxTriggered();
    }
}