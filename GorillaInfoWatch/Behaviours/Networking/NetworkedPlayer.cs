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
        public InfoWatch InfoWatch => watch;

        public bool HasInfoWatch;

        public VRRig Rig;
        public NetPlayer Owner;

        private InfoWatch watch;

        public void Start()
        {
            // https://github.com/The-Graze/Grate/blob/9dddf2084a75f22cc45024f38d564f788db661d6/Networking/NetworkedPlayer.cs#L39
            NetworkHandler.Instance.OnPlayerPropertyChanged += OnPlayerPropertyChanged;

            if (!HasInfoWatch && Owner is PunNetPlayer punPlayer && punPlayer.PlayerRef is Player playerRef)
                NetworkHandler.Instance.OnPlayerPropertiesUpdate(playerRef, playerRef.CustomProperties);
        }

        public void OnDestroy()
        {
            NetworkHandler.Instance.OnPlayerPropertyChanged -= OnPlayerPropertyChanged;

            if (HasInfoWatch)
            {
                HasInfoWatch = false;
                Destroy(watch.gameObject);
            }
        }

        public void OnPlayerPropertyChanged(NetPlayer player, Dictionary<string, object> properties)
        {
            if (player == Owner)
            {
                Logging.Info($"{player.NickName} got properties: {string.Join(", ", properties.Select(prop => $"[{prop.Key}: {prop.Value}]"))}");

                if (watch is null)
                {
                    CreateWatch();
                }

                if (properties.TryGetValue("TimeOffset", out object time_offset_object) && time_offset_object is float time_offset)
                {
                    watch.TimeOffset = time_offset;
                }
            }
        }

        public void CreateWatch()
        {
            watch = Instantiate(Singleton<Main>.Instance.WatchAsset).AddComponent<InfoWatch>();
            watch.Rig = Rig;
        }
    }
}