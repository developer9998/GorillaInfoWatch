using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Screens;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using GorillaNetworking;
using Photon.Pun;
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

            RegisterShortcut("Leave", "Disconnects you from the current room", () =>
            {
                if (!NetworkSystem.Instance.InRoom) return;
                NetworkSystem.Instance.ReturnToSinglePlayer();
            });

            RegisterShortcut("Rejoin", "Reconnects you to the current room", async () =>
            {
                if (!NetworkSystem.Instance.InRoom) return;

                string roomName = NetworkSystem.Instance.RoomName;

                await NetworkSystem.Instance.ReturnToSinglePlayer();
                await Task.Delay(250);

                await PhotonNetworkController.Instance.AttemptToJoinSpecificRoomAsync(roomName, JoinType.Solo, null);
            });

            RegisterShortcut("Copy Room", "Copies the name of the current room to your clipboard", () =>
            {
                if (NetworkSystem.Instance.InRoom)
                {
                    string roomName = NetworkSystem.Instance.RoomName;
                    GUIUtility.systemCopyBuffer = roomName;
                    Notify("Room Name", roomName, 2f);
                }
            });

            RegisterShortcut("Get Region", "Gets the region of the current room", () =>
            {
                if (!NetworkSystem.Instance.InRoom) return;

                string regionCode = NetworkSystem.Instance.CurrentRegion.Replace("/*", "").ToLower();
                string regionName = regionCode switch
                {
                    "us" => "United States (East)", // Washington DC
                    "usw" => "United States (West)", // San Jose
                    "eu" => "Europe", // Amsterdam

                    // Region codes below aren't used by Gorilla Tag as of commenting this
                    // Server locations were only ever expanded in May 2021 (including US West and Europe) and never again
                    "asia" => "Asia", // Singapore
                    "au" => "Australia", // Sydney
                    "cae" => "Canada", // Montreal
                    "hk" => "Hong Kong", // Hong Kong
                    "in" => "India", // Chennai
                    "jp" => "Japan", // Tokyo
                    "za" => "South Africa", // Johannesburg
                    "sa" => "South America", // Sao Paulo
                    "kr" => "South Korea", // Seoul
                    "tr" => "Turkey", // Istanbul
                    "uae" => "United Arab Emirates", // Dubai
                    "ussc" => "United States (South Central)", // Dallas
                    _ => throw new ArgumentOutOfRangeException()
                };

                Notify("Room Region", regionName, 2);
            });

            RegisterShortcut("Get Room Host", "Gets the room host of the current room", () =>
            {
                if (!NetworkSystem.Instance.InRoom) return;

                NetPlayer masterClient = NetworkSystem.Instance.MasterClient;
                string userId = masterClient.UserId;

                Notify("Room Host", masterClient.GetName().EnforcePlayerNameLength(), 4, screen: new Notification.ExternalScreen(typeof(PlayerInspectorScreen), "Inspect Player", () =>
                {
                    masterClient = PlayerUtility.GetPlayer(userId);
                    if (masterClient != null && !masterClient.IsNull) PlayerInspectorScreen.UserId = userId;
                }));
            });

            RegisterShortcut("Get Ping", "Gets the round trip time to the current server, known as your ping", () =>
            {
                if (PhotonNetwork.NetworkingClient is not LoadBalancingClient client || client.LoadBalancingPeer is not LoadBalancingPeer peer) return;
                Notify($"{peer.RoundTripTime} ms", 1);
            });
        }
    }
}
