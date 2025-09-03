using GorillaInfoWatch.Extensions;
using GorillaInfoWatch.Tools;
using Photon.Realtime;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.Networking
{
    [RequireComponent(typeof(RigContainer)), DisallowMultipleComponent]
    public class NetworkedPlayer : MonoBehaviour
    {
        public InfoWatch NetworkedInfoWatch { get; private set; }

        public bool HasInfoWatch { get; set; }

        public VRRig Rig;

        public NetPlayer Player;

        public void Start()
        {
            NetworkManager.Instance.OnPlayerPropertyChanged += OnPlayerPropertyChanged;

            if (!HasInfoWatch && Player is PunNetPlayer punPlayer && punPlayer.PlayerRef is Player playerRef)
                NetworkManager.Instance.OnPlayerPropertiesUpdate(playerRef, playerRef.CustomProperties);
        }

        public void OnDestroy()
        {
            NetworkManager.Instance.OnPlayerPropertyChanged -= OnPlayerPropertyChanged;

            if (HasInfoWatch)
            {
                HasInfoWatch = false;
                if (NetworkedInfoWatch) Destroy(NetworkedInfoWatch.gameObject);
            }
        }

        public void OnPlayerPropertyChanged(NetPlayer player, Dictionary<string, object> properties)
        {
            if (player == Player)
            {
                Logging.Info($"{player.GetName().SanitizeName()} got properties: {string.Join(", ", properties.Select(prop => $"[{prop.Key}: {prop.Value}]"))}");

                if (NetworkedInfoWatch == null || !NetworkedInfoWatch)
                {
                    GameObject prefab = Instantiate(Main.Content.WatchPrefab);
                    NetworkedInfoWatch = prefab.GetComponent<InfoWatch>();
                    NetworkedInfoWatch.Rig = Rig;
                    prefab.SetActive(true);
                }

                if (properties.TryGetValue("TimeOffset", out object timeOffsetObj) && timeOffsetObj is float timeObject)
                    NetworkedInfoWatch.TimeOffset = timeObject;
            }
        }
    }
}