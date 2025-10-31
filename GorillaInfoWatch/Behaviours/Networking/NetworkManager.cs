using System;
using System.Collections.Generic;
using System.Linq;
using ExitGames.Client.Photon;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Tools;
using Photon.Pun;
using Photon.Realtime;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.Networking;

public class NetworkManager : MonoBehaviourPunCallbacks
{
    private readonly Dictionary<string, object> properties = [];

    public Action<NetPlayer, Dictionary<string, object>> OnPlayerPropertyChanged;

    private float propertyTimer;

    private       bool           setProperties;
    public static NetworkManager Instance { get; private set; }

    public void Awake()
    {
        if (Instance != null && Instance != this)
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
            Application.quitting += delegate
                                    {
                                        Logging.Message("Disabling NetworkHandler");
                                        PhotonNetwork.RemoveCallbackTarget(this);
                                    };

            return;
        }

        Logging.Warning(
                "Disabling NetworkHandler, either NetworkSystem.Instance is null or isn't based on NetworkSystemPUN");

        enabled = false;
    }

    public void LateUpdate()
    {
        propertyTimer -= Time.deltaTime;

        if (setProperties && properties.Count > 0 && propertyTimer <= 0)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new Hashtable
            {
                    {
                            Constants.NetworkPropertyKey,
                            new Dictionary<string, object>(properties)
                    },
            });

            setProperties = false;
            propertyTimer = Constants.NetworkRaiseInterval;
        }
    }

    public void SetProperty(string key, object value)
    {
        if (properties.ContainsKey(key))
        {
            bool isEquivalent = value.Equals(properties[key]);
            properties[key] = value;
            setProperties   = isEquivalent || setProperties;

            return;
        }

        properties.Add(key, value);
        setProperties = true;
    }

    public void RemoveProperty(string key)
    {
        if (properties.ContainsKey(key))
        {
            properties.Remove(key);
            setProperties = true;
        }
    }

    public override void OnPlayerPropertiesUpdate(Player targetPlayer, Hashtable changedProps)
    {
        NetPlayer netPlayer = NetworkSystem.Instance.GetPlayer(targetPlayer.ActorNumber);

        if (netPlayer.IsLocal || !VRRigCache.rigsInUse.TryGetValue(netPlayer, out RigContainer playerRig) ||
            !playerRig.TryGetComponent(out NetworkedPlayer networkedPlayer))
            return;

        if (changedProps.TryGetValue(Constants.NetworkPropertyKey, out object propertiesObject) &&
            propertiesObject is Dictionary<string, object> properties)
        {
            if (!networkedPlayer.HasInfoWatch)
            {
                Logging.Message($"{netPlayer.GetName()} has GorillaInfoWatch");
                networkedPlayer.HasInfoWatch = true;
                SignificanceManager.Instance.CheckPlayer(netPlayer, SignificanceCheckScope.InfoWatch);
            }

            Logging.Message($"Recieved properties from {netPlayer.NickName}");
            Logging.Info(string.Join(Environment.NewLine, properties.Select(prop => $"[{prop.Key}, {prop.Value}]")));
            OnPlayerPropertyChanged?.Invoke(netPlayer, properties);
        }
    }
}