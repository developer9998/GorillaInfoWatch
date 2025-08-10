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
        public bool HasInfoWatch;

        public VRRig Rig;
        public NetPlayer Owner;

        private InfoWatch playerInfoWatch;

        public void Start()
        {
            NetworkManager.Instance.OnPlayerPropertyChanged += OnPlayerPropertyChanged;

            if (!HasInfoWatch && Owner is PunNetPlayer punPlayer && punPlayer.PlayerRef is Player playerRef)
                NetworkManager.Instance.OnPlayerPropertiesUpdate(playerRef, playerRef.CustomProperties);
        }

        public void OnDestroy()
        {
            NetworkManager.Instance.OnPlayerPropertyChanged -= OnPlayerPropertyChanged;

            if (!HasInfoWatch) return;
            HasInfoWatch = false;

            if (playerInfoWatch is not null && playerInfoWatch) Destroy(playerInfoWatch.gameObject);
        }

        public void OnPlayerPropertyChanged(NetPlayer player, Dictionary<string, object> properties)
        {
            if (player == Owner)
            {
                Logging.Info($"{player.GetNameRef().SanitizeName()} got properties: {string.Join(", ", properties.Select(prop => $"[{prop.Key}: {prop.Value}]"))}");

                if (playerInfoWatch is null) CreateWatch();

                if (properties.TryGetValue("TimeOffset", out object timeOffsetObj) && timeOffsetObj is float timeObject)
                    playerInfoWatch.TimeOffset = timeObject;
            }
        }

        public void CreateWatch()
        {
            if (playerInfoWatch is not null && playerInfoWatch) return;

            GameObject gameObject = Instantiate(Main.Content.WatchPrefab);
            playerInfoWatch = gameObject.GetComponent<InfoWatch>();
            playerInfoWatch.Rig = Rig;
            gameObject.SetActive(true);
        }
    }
}