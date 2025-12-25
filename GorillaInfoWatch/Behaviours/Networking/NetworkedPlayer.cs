using GorillaInfoWatch.Behaviours.UI;
using GorillaInfoWatch.Extensions;
using Photon.Realtime;
using System.Collections.Generic;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.Networking;

[RequireComponent(typeof(RigContainer)), DisallowMultipleComponent]
public class NetworkedPlayer : MonoBehaviour, IPreDisable
{
    public Watch Watch { get; private set; }
    public bool HasInfoWatch { get; set; }

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

        if (HasInfoWatch)
        {
            HasInfoWatch = false;
            Watch?.Obliterate();
        }
    }

    public void OnPlayerPropertyChanged(NetPlayer player, Dictionary<string, object> properties)
    {
        if (!enabled || player != Player) return;

        if (Watch.Null())
        {
            GameObject prefab = Instantiate(Main.Content.WatchPrefab);
            Watch = prefab.GetComponent<Watch>();
            Watch.Rig = Rig;
            prefab.SetActive(true);
        }

        if (properties.TryGetValue("TimeOffset", out object timeOffsetObj) && timeOffsetObj is float timeObject)
        {
            Watch.TimeOffset = timeObject;
        }
    }

    public void PreDisable()
    {
        enabled = false;

        if (HasInfoWatch)
        {
            HasInfoWatch = false;
            Watch?.Obliterate();
        }
    }
}