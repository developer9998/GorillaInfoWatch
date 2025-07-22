using ExitGames.Client.Photon;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Tools;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.Networking
{
    public class NetworkManager : MonoBehaviourPunCallbacks
    {
        public static NetworkManager Instance { get; private set; }

        public Action<NetPlayer, Dictionary<string, object>> OnPlayerPropertyChanged;

        private readonly Dictionary<string, object> properties = [];
        private bool set_properties = false;
        private float properties_timer;

        public void Awake()
        {
            if (Instance != null && (bool)Instance && Instance != this)
            {
                Destroy(this);
                return;
            }

            Instance = this;

            if (NetworkSystem.Instance is NetworkSystem netSys && netSys is NetworkSystemPUN)
            {
                Logging.Message("Setting up NetworkHandler");
                SetProperty("Version", Constants.Version);

                PhotonNetwork.AddCallbackTarget(this);
                Application.quitting += delegate ()
                {
                    Logging.Message("Disabling NetworkHandler");
                    PhotonNetwork.RemoveCallbackTarget(this);
                };
                return;
            }

            Logging.Warning("Disabling NetworkHandler, either NetworkSystem.Instance is null or isn't based on NetworkSystemPUN");
            enabled = false;
        }

        public void Update()
        {
            properties_timer -= Time.deltaTime;

            if (set_properties && properties.Count > 0 && properties_timer <= 0)
            {
                PhotonNetwork.LocalPlayer.SetCustomProperties(new()
                {
                    {
                        Constants.NetworkPropertyKey,
                        new Dictionary<string, object>(properties)
                    }
                });

                set_properties = false;
                properties_timer = Constants.NetworkRaiseInterval;
            }
        }

        public void SetProperty(string key, object value)
        {
            if (properties.ContainsKey(key)) properties[key] = value;
            else properties.Add(key, value);
            set_properties = true;
        }


        public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
        {
            NetPlayer netPlayer = NetworkSystem.Instance.GetPlayer(targetPlayer.ActorNumber);

            if (netPlayer.IsLocal || !VRRigCache.Instance.TryGetVrrig(netPlayer, out RigContainer playerRig) || !playerRig.TryGetComponent(out NetworkedPlayer networkedPlayer))
                return;

            if (changedProps.TryGetValue(Constants.NetworkPropertyKey, out object propertiesObject) && propertiesObject is Dictionary<string, object> properties)
            {
                if (!networkedPlayer.HasInfoWatch)
                {
                    Logging.Message($"{netPlayer.GetNameRef()} has GorillaInfoWatch");
                    networkedPlayer.HasInfoWatch = true;
                    Main.Instance.CheckPlayer(netPlayer);
                }

                Logging.Message($"Recieved properties from {netPlayer.NickName}");
                Logging.Info(string.Join(Environment.NewLine, properties.Select(prop => $"[{prop.Key}, {prop.Value}]")));
                OnPlayerPropertyChanged?.Invoke(netPlayer, properties);

                return;
            }
        }
    }
}