using ExitGames.Client.Photon;
using GorillaInfoWatch.Tools;
using Photon.Pun;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.Networking;

internal class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

    public Action<NetPlayer, Dictionary<string, object>> OnPlayerPropertyChanged;

    private readonly Dictionary<string, object> properties = [];

    private bool setProperties = false;

    private float propertyTimer;

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

    public void LateUpdate()
    {
        propertyTimer -= Time.deltaTime;

        if (setProperties && properties.Count > 0 && propertyTimer <= 0)
        {
            PhotonNetwork.LocalPlayer.SetCustomProperties(new()
            {
                {
                    Constants.NetworkPropertyKey,
                    new Dictionary<string, object>(properties)
                }
            });

            setProperties = false;
            propertyTimer = Constants.NetworkRaiseInterval;
        }
    }

    public void SetProperty(string key, object value)
    {
        if (properties.ContainsKey(key)) properties[key] = value;
        else properties.Add(key, value);
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

        if (netPlayer != null && !netPlayer.IsLocal && changedProps.TryGetValue(Constants.NetworkPropertyKey, out object propertiesObject) && propertiesObject is Dictionary<string, object> properties) OnPlayerPropertyChanged?.Invoke(netPlayer, properties);
    }
}