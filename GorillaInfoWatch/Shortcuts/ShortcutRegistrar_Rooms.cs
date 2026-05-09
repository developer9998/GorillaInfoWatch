using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Models.Shortcuts;
using GorillaInfoWatch.Models.UserInput;
using GorillaInfoWatch.Tools;
using GorillaNetworking;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using UnityEngine;
using Random = UnityEngine.Random;

namespace GorillaInfoWatch.Shortcuts;

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

        RegisterShortcut("Join Random Room", "Joins a random public room under a map that is loaded", async () =>
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

        RegisterShortcut("Join Specific Room", "Joins a specific room of the name provided by the player", () =>
        {
            UserInput.Activate(GorillaComputer.instance.roomToJoin, UserInputBoard.Standard, 10, (sender, args) =>
            {
                string roomCode = args.Input;
                GorillaComputer.instance.roomToJoin = roomCode;

                if (!args.IsTyping)
                {
                    if (GorillaComputer.instance.currentStateIndex != 0)
                    {
                        GorillaComputer.instance.currentStateIndex = 0;
                        GorillaComputer.instance.SwitchState(GorillaComputer.instance.GetState(GorillaComputer.instance.currentStateIndex), true);
                    }
                    GorillaComputer.instance.ProcessRoomState(GorillaKeyboardBindings.enter);
                }
            });
        });

        RegisterShortcut("Leave", "The current room containing the player is left", ShortcutRestrictions.Multiplayer, () =>
        {
            if (!NetworkSystem.Instance.InRoom) return;
            NetworkSystem.Instance.ReturnToSinglePlayer();
        });

        RegisterShortcut("Rejoin", "The current room containing the player is re-joined (left and then joined)", ShortcutRestrictions.Multiplayer, async () =>
        {
            if (!NetworkSystem.Instance.InRoom) return;

            string roomName = NetworkSystem.Instance.RoomName;

            await NetworkSystem.Instance.ReturnToSinglePlayer();
            await Task.Delay(250);
            await PhotonNetworkController.Instance.AttemptToJoinSpecificRoomAsync(roomName, JoinType.Solo, null);
        });

        RegisterShortcut("Copy Room Name", "Copies the name of the current room to your clipboard", ShortcutRestrictions.Multiplayer, () =>
        {
            if (NetworkSystem.Instance.InRoom)
            {
                string roomName = NetworkSystem.Instance.RoomName;
                GUIUtility.systemCopyBuffer = roomName;
            }
        });
    }
}