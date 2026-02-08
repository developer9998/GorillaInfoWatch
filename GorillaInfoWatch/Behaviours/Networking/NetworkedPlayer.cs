using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Tools;
using GorillaLocomotion;
using GorillaNetworking;
using Photon.Realtime;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.Networking;

[RequireComponent(typeof(RigContainer)), DisallowMultipleComponent]
public class NetworkedPlayer : MonoBehaviour, IPreDisable
{
    public bool HasInfoWatch { get; set; }
    public Watch Watch { get; private set; }
    public PlayerConsent Consent { get; private set; }

    public VRRig Rig;

    public NetPlayer Player;

    private static readonly Dictionary<string, PlayerConsent> _consentCache = [];

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
                PlayerConsent consent = (PlayerConsent)Enum.ToObject(typeof(PlayerConsent), consentInteger);
                Consent = consent;
                _consentCache.AddOrUpdate(Player.UserId, consent);
                SignificanceManager.Instance.CheckPlayer(Player, SignificanceCheckScope.Item | SignificanceCheckScope.Figure);
            }
        }
        catch(Exception ex)
        {
            Logging.Fatal("Failed to make changes from properties");
            Logging.Error(ex);
        }
    }

    public static PlayerConsent GetTemporaryConsent(string userId) => _consentCache.TryGetValue(userId ?? "", out PlayerConsent consent) ? consent : PlayerConsent.None;

    public static void RemoveTemporaryConsent(string userId)
    {
        if (!_consentCache.ContainsKey(userId)) return;
        _consentCache.Remove(userId);
    }
}