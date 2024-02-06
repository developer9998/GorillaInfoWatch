using GorillaInfoWatch.Models;
using System;

namespace GorillaInfoWatch
{
    public class Events
    {
        public static event Action<PlayerInfo> RigAdded, RigRemoved;

        public virtual void TriggerRigAdded(PlayerInfo args) => RigAdded?.Invoke(args);
        public virtual void TriggerRigRemoved(PlayerInfo args) => RigRemoved?.Invoke(args);
    }
}
