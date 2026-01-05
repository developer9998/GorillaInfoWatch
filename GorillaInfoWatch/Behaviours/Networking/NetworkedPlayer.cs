using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Tools;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.Networking;

[RequireComponent(typeof(RigContainer)), DisallowMultipleComponent]
public class NetworkedPlayer : MonoBehaviour, IPreDisable
{
    public bool HasInfoWatch { get; private set; }
    public Watch Watch { get; private set; }
    public SignificanceVisibility Consent { get; private set; }

    public VRRig Rig;

    public NetPlayer Player;

    public void Start()
    {
        NetworkManager.Instance.OnPlayerPropertyChanged += OnPlayerPropertyChanged;

        if (!HasInfoWatch && Player is PunNetPlayer punPlayer && punPlayer.PlayerRef is Player playerRef) NetworkManager.Instance.OnPlayerPropertiesUpdate(playerRef, playerRef.CustomProperties);
    }

    public void OnDestroy()
    {
        NetworkManager.Instance.OnPlayerPropertyChanged -= OnPlayerPropertyChanged;

        HasInfoWatch = false;
        if (Watch.Exists()) Watch.gameObject.Obliterate();
    }

    public void PreDisable()
    {
        enabled = false;
        HasInfoWatch = false;
        if (Watch.Exists()) Watch.gameObject.Obliterate();
    }

    public void OnPlayerPropertyChanged(NetPlayer player, Dictionary<string, object> properties)
    {
        if (!enabled || player != Player) return;

        if (!HasInfoWatch)
        {
            HasInfoWatch = true;

            Logging.Message($"{Player.GetName()} has GorillaInfoWatch");
            SignificanceManager.Instance.CheckPlayer(Player, SignificanceCheckScope.InfoWatch);
        }

        Logging.Message($"{player.NickName} has updated properties:");
        properties.ForEach(element => Logging.Info($"{element.Key}: {element.Value}"));

        if (Watch.Null())
        {
            GameObject prefab = Instantiate(Main.Content.WatchPrefab);
            Watch = prefab.GetComponent<Watch>();
            Watch.Rig = Rig;
            prefab.SetActive(true);
        }

        try
        {
            if (properties.TryGetValue("TimeOffset", out object timeOffsetObj) && timeOffsetObj is float timeOffset)
            {
                Watch.TimeOffset = timeOffset;
            }

            if (properties.TryGetValue("Orientation", out object orientationObj) && orientationObj is bool orientation)
            {
                Watch.SetHand(orientation);
            }

            if (properties.TryGetValue("Consent", out object consentObj) && consentObj is int consentInteger)
            {
                SignificanceVisibility consent = (SignificanceVisibility)Enum.ToObject(typeof(SignificanceVisibility), consentInteger);
                Consent = consent;
                SignificanceManager.Instance.CheckPlayer(Player, SignificanceCheckScope.Item | SignificanceCheckScope.Figure);
            }
        }
        catch (Exception ex)
        {
            Logging.Fatal("Failed to make changes from properties");
            Logging.Error(ex);
        }
    }
}