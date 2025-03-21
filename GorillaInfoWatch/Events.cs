using System;

namespace GorillaInfoWatch
{
    public class Events
    {
        public static Events Instance = new();

        public static event Action<NetPlayer, VRRig> OnPlayerJoined;

        public static event Action<NetPlayer, VRRig> OnPlayerLeft;

        public virtual void PlayerJoined(NetPlayer player, VRRig rig) => OnPlayerJoined?.Invoke(player, rig);

        public virtual void PlayerLeft(NetPlayer player, VRRig rig) => OnPlayerLeft?.Invoke(player, rig);
    }
}