using GorillaInfoWatch.Models;
using System;

namespace GorillaInfoWatch
{
    public class Events
    {
        public static event Action<PlayerArgs> RigAdded, RigRemoved;

        public virtual void TriggerRigAdded(PlayerArgs args) => RigAdded?.Invoke(args);
        public virtual void TriggerRigRemoved(PlayerArgs args) => RigRemoved?.Invoke(args);
    }
}
