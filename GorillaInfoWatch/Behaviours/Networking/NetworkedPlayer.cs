using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Models;
using GorillaInfoWatch.Models.Significance;
using GorillaInfoWatch.Tools;
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
        if (HasInfoWatch) return;

        if (Player is PunNetPlayer punPlayer && punPlayer.PlayerRef is Player playerRef) NetworkManager.Instance.OnPlayerPropertiesUpdate(playerRef, playerRef.CustomProperties);
    }

    public void OnDestroy()
    {
        HasInfoWatch = false;
        if (Watch.Exists()) Watch.gameObject.Obliterate();
    }

    public void PreDisable()
    {
        enabled = false;
        HasInfoWatch = false;
        if (Watch.Exists()) Watch.gameObject.Obliterate();
    }

    public void OnPlayerPropertyChanged(Dictionary<string, object> properties)
    {
        if (!enabled) return;

        if (Watch.Null())
        {
            GameObject prefab = Instantiate(Main.Content.WatchPrefab);
            Watch = prefab.GetComponent<Watch>();
            Watch.Rig = Rig;
            prefab.SetActive(true);
        }

        if (properties.TryGetValue("TimeOffset", out object timeOffsetObj) && timeOffsetObj is float timeOffset)
        {
            Watch.TimeOffset = timeOffset;
        }

        if (properties.TryGetValue("Orientation", out object orientationObj) && orientationObj is bool orientation)
        {
            Watch.SetHand(orientation);
        }

        if (properties.TryGetValue("Consent", out object consentObj) && consentObj is PlayerConsent consent)
        {
            SignificanceCheckScope scope = SignificanceCheckScope.None;

            Dictionary<PlayerConsent, SignificanceCheckScope> dictionary = new()
            {
                { PlayerConsent.Item, SignificanceCheckScope.Item },
                { PlayerConsent.Figure, SignificanceCheckScope.Figure }
            };

            long totalConsentInteger = Convert.ToInt64(Consent);
            long newConsentInteger = Convert.ToInt64(consent);

            dictionary.Where(element =>
            {
                long flagInteger = Convert.ToInt64(element.Key);
                long totalContains = totalConsentInteger & flagInteger;
                long newContains = newConsentInteger & flagInteger;
                Logging.Info($"Consent includes {element.Key}: {newContains == flagInteger}");
                return totalContains != newContains;
            }).ForEach(element => scope |= element.Value);

            Consent = consent;
            _consentCache.AddOrUpdate(Player.UserId, consent);
            SignificanceManager.Instance.CheckPlayer(Player, scope);
        }
    }

    public static PlayerConsent GetTemporaryConsent(string userId) => _consentCache.TryGetValue(userId ?? "", out PlayerConsent consent) ? consent : PlayerConsent.None;

    public static void RemoveTemporaryConsent(string userId)
    {
        if (!_consentCache.ContainsKey(userId)) return;
        _consentCache.Remove(userId);
    }
}