using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using GorillaInfoWatch.Tools;
using GorillaInfoWatch.Utilities;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.Networking
{
    // https://github.com/KyleTheScientist/Bark/blob/3de171aca033d45f464a5120fb1932c9a0d2a3af/Networking/NetworkPropertyHandler.cs
    public class NetworkHandler : Singleton<NetworkHandler>, IInRoomCallbacks
    {
        public Action<NetPlayer, Dictionary<string, object>> OnPlayerPropertyChanged;

        private readonly Dictionary<string, object> properties = [];
        private bool set_properties = false;
        private float properties_timer;

        public override void Initialize()
        {
            if (NetworkSystem.Instance && (NetworkSystem.Instance.CurrentPhotonBackend == "PUN" || NetworkSystem.Instance is NetworkSystemPUN))
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new()
                { 
                    { Constants.NetworkVersionKey, Constants.Version }
                });
                return;
            }

            enabled = false;
        }

        public void FixedUpdate()
        {
            /*if (!PhotonNetwork.InRoom)
            {
                if (properties_timer < 0.2f) properties_timer = 0.2f;
                return;
            }*/

            properties_timer -= Time.deltaTime;

            if (set_properties && properties.Count > 0 && properties_timer <= 0)
            {
                Logging.Info($"Set properties for local player: {string.Join(", ", properties.Select(prop => $"[{prop.Key}: {prop.Value}]"))}");
                PhotonNetwork.LocalPlayer.SetCustomProperties(new() 
                { 
                    { Constants.NetworkPropertiesKey, new Dictionary<string, object>(properties) } 
                });
                set_properties = false;
                properties_timer = Constants.NetworkSetInterval;
            }
        }

        public void SetProperty(string key, object value)
        {
            if (properties.ContainsKey(key)) properties[key] = value;
            else properties.Add(key, value);
            set_properties = true;
        }

        /*public override void OnLeftRoom()
        {
            set_properties = true;
            properties_timer = 2;
        }*/

        public void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            // https://github.com/The-Graze/Grate/blob/9dddf2084a75f22cc45024f38d564f788db661d6/Networking/NetworkPropertyHandler.cs#L45C13-L45C100
            NetPlayer target_player = NetworkSystem.Instance.GetPlayer(targetPlayer.ActorNumber);
            if (changedProps.TryGetValue(Constants.NetworkPropertiesKey, out object props_object) && props_object is Dictionary<string, object> properties)
            {
                bool is_local_player = target_player.IsLocal;
                Logging.Info($"Recieved properties from {target_player.NickName}{(is_local_player ? " (local player)" : "")}: {string.Join(", ", properties.Select(prop => $"[{prop.Key}: {prop.Value}]"))}");
                if (!is_local_player)
                {
                    if (RigUtils.TryGetVRRig(target_player, out RigContainer rigContainer) && rigContainer.TryGetComponent(out NetworkedPlayer networked_player))
                    {
                        networked_player.HasWatch = true;
                        Logging.Info($"{target_player.NickName} has watch");
                    }
                    OnPlayerPropertyChanged?.Invoke(target_player, properties);
                }
            }
        }

        public void OnPlayerEnteredRoom(Player newPlayer)
        {
            
        }

        public void OnPlayerLeftRoom(Player otherPlayer)
        {
            
        }

        public void OnRoomPropertiesUpdate(Hashtable propertiesThatChanged)
        {
            
        }

        public void OnMasterClientSwitched(Player newMasterClient)
        {
            
        }
    }
}