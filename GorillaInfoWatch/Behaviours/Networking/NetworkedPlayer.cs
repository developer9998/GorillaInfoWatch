using System.Collections.Generic;
using System.Linq;
using GorillaInfoWatch.Tools;
using Photon.Realtime;
using UnityEngine;

namespace GorillaInfoWatch.Behaviours.Networking
{
    [RequireComponent(typeof(RigContainer)), DisallowMultipleComponent]
    public class NetworkedPlayer : MonoBehaviour
    {
        public VRRig Rig;
        public NetPlayer Owner;
        public bool HasWatch;

        private Watch watch;

        public void Start()
        {
            // https://github.com/The-Graze/Grate/blob/9dddf2084a75f22cc45024f38d564f788db661d6/Networking/NetworkedPlayer.cs#L39
            NetworkHandler.Instance.OnPlayerPropertyChanged += OnPlayerPropertyChanged;

            Player player = Owner.GetPlayerRef();
            NetworkHandler.Instance.OnPlayerPropertiesUpdate(player, player.CustomProperties);
        }

        public void OnDestroy()
        {
            NetworkHandler.Instance.OnPlayerPropertyChanged -= OnPlayerPropertyChanged;

            if (HasWatch)
            {
                HasWatch = false;
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
            watch = Instantiate(Singleton<Main>.Instance.WatchAsset).AddComponent<Watch>();
            watch.Rig = Rig;
        }
    }
}