using ExitGames.Client.Photon;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Tools;
using Photon.Pun;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.Networking;

internal class NetworkManager : MonoBehaviourPunCallbacks
{
    public static NetworkManager Instance { get; private set; }

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
        if (properties.ContainsKey(key))
        {
            bool isEquivalent = value.Equals(properties[key]);
            properties[key] = value;
            setProperties = isEquivalent || setProperties;
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

        if (netPlayer.IsLocal || !VRRigCache.rigsInUse.TryGetValue(netPlayer, out RigContainer playerRig) || !playerRig.TryGetComponent(out NetworkedPlayer networkedPlayer) || !changedProps.TryGetValue(Constants.NetworkPropertyKey, out object propertiesObject) || propertiesObject is not Dictionary<string, object> properties)
            return;

        Logging.Message($"{netPlayer.NickName} has updated properties:");
        properties.ForEach(element => Logging.Info($"{element.Key}: {element.Value}"));

        if (!networkedPlayer.HasInfoWatch)
        {
            networkedPlayer.HasInfoWatch = true;

            Logging.Message($"{netPlayer.GetName()} has GorillaInfoWatch");
            SignificanceManager.Instance.CheckPlayer(netPlayer, SignificanceCheckScope.InfoWatch);
        }

        networkedPlayer.OnPlayerPropertyChanged(properties);
    }
}