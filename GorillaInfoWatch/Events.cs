using Photon.Realtime;
using System;

namespace GorillaInfoWatch
{
    public class Events
    {
        public static event Action<Player, VRRig> RigAdded, RigRemoved;

        public virtual void TriggerRigAdded(Player player, VRRig vrRig) => RigAdded?.Invoke(player, vrRig);
        public virtual void TriggerRigRemoved(Player player, VRRig vrRig) => RigRemoved?.Invoke(player, vrRig);
    }
}
