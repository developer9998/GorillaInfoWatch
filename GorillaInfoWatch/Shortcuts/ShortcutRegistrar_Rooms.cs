using GorillaInfoWatch.Models;
using GorillaInfoWatch.Tools;
using GorillaNetworking;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Random = UnityEngine.Random;

namespace GorillaInfoWatch.Functions
{
    internal class ShortcutRegistrar_Rooms : ShortcutRegistrar
    {
        public override string Title => "Rooms";

        public ShortcutRegistrar_Rooms()
        {
            RegisterShortcut("Join Random Room", "You will join an available public room", async () =>
            {
                List<GTZone> activeZones = ZoneManagement.instance.activeZones;

                List<GorillaNetworkJoinTrigger> triggers = [];

                foreach (var (networkZone, joinTrigger) in GorillaComputer.instance.primaryTriggersByZone)
                {
                    if (joinTrigger == null) continue;

                    if (activeZones.Contains(joinTrigger.zone) && (GorillaComputer.instance.friendJoinCollider == null || GorillaComputer.instance.friendJoinCollider.myAllowedMapsToJoin.Contains(networkZone)))
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
            });

            RegisterShortcut("Disconnect", "You will be disconnected from the current room", () =>
            {
                if (!NetworkSystem.Instance.InRoom) return;
                NetworkSystem.Instance.ReturnToSinglePlayer();
            });

            RegisterShortcut("Rejoin Room", "You will reconnect to your current room", async () =>
            {
                if (!NetworkSystem.Instance.InRoom) return;

                string roomName = NetworkSystem.Instance.RoomName;

                await NetworkSystem.Instance.ReturnToSinglePlayer();
                await Task.Delay(250);

                await PhotonNetworkController.Instance.AttemptToJoinSpecificRoomAsync(roomName, JoinType.Solo, null);
            });
        }
    }
}
