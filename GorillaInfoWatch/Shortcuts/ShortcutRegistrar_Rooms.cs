using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Screens;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using GorillaNetworking;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GorillaInfoWatch.Shortcuts
{
    internal class ShortcutRegistrar_Rooms : ShortcutRegistrar
    {
        public override string Title => "Rooms";

        private readonly Stack<string> _roomNameStack = [];

        private string _lastRoomName = null;

        public ShortcutRegistrar_Rooms()
        {
            NetworkSystem.Instance.OnMultiplayerStarted += () =>
            {
                _lastRoomName = NetworkSystem.Instance.RoomName;
            };

            NetworkSystem.Instance.OnReturnedToSinglePlayer += () =>
            {
                if (_lastRoomName != null)
                {
                    _roomNameStack.Push(_lastRoomName);
                    _lastRoomName = null;
                }
            };

            RegisterShortcut("Join Random Room", "Joins a random public room for a loaded map", async () =>
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
            });

            RegisterShortcut("Leave", "Leave the room you are currently in", ShortcutRestrictions.Multiplayer, () =>
            {
                if (!NetworkSystem.Instance.InRoom) return;
                NetworkSystem.Instance.ReturnToSinglePlayer();
            });

            RegisterShortcut("Rejoin", "Leaves then joins your current room", ShortcutRestrictions.Multiplayer, async () =>
            {
                if (!NetworkSystem.Instance.InRoom) return;

                string roomName = NetworkSystem.Instance.RoomName;

                await NetworkSystem.Instance.ReturnToSinglePlayer();
                await Task.Delay(250);

                await PhotonNetworkController.Instance.AttemptToJoinSpecificRoomAsync(roomName, JoinType.Solo, null);
            });

            RegisterShortcut("Copy Room", "Copies the name of your current room to the clipboard", ShortcutRestrictions.Multiplayer, () =>
            {
                if (NetworkSystem.Instance.InRoom)
                {
                    string roomName = NetworkSystem.Instance.RoomName;
                    GUIUtility.systemCopyBuffer = roomName;
                    Notify("Room Name", roomName, 2f);
                }
            });

            RegisterShortcut("Identify Master", "Identifies the master client of your current room", ShortcutRestrictions.Multiplayer, () =>
            {
                if (!NetworkSystem.Instance.InRoom) return;

                NetPlayer masterClient = NetworkSystem.Instance.MasterClient;
                string userId = masterClient.UserId;

                Notify("Master Client", masterClient.GetName().EnforcePlayerNameLength(), 4, screen: new Notification.ExternalScreen(typeof(PlayerInspectorScreen), "Inspect Player", () =>
                {
                    masterClient = PlayerUtility.GetPlayer(userId);
                    if (masterClient != null && !masterClient.IsNull) PlayerInspectorScreen.UserId = userId;
                }));
            });

            /*
            RegisterShortcut("Mute Player", "Changes the mute state applied to the nearest player", () =>
            {
                if (!NetworkSystem.Instance.InRoom) return;

                List<RigContainer> rigsInUse = [.. VRRigCache.rigsInUse.Values];
                if (rigsInUse.Count == 0) return;

                Vector3 position = GTPlayer.Instance.HeadCenterPosition;

                float nearestDistance = -1;
                int nearestIndex = 0;

                for (int i = 0; i < rigsInUse.Count; i++)
                {
                    float distance = (position - rigsInUse[i].Rig.syncPos).sqrMagnitude;
                    if (nearestDistance == -1 || nearestDistance > distance)
                    {
                        nearestDistance = distance;
                        nearestIndex = i;
                    }
                }

                RigContainer nearest = rigsInUse[nearestIndex];
                PlayerUtility.MutePlayer(nearest.Creator, !nearest.Muted);
            });
            */
        }
    }
}