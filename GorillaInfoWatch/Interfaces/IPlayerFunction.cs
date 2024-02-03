using Photon.Realtime;
using System;

namespace GorillaInfoWatch.Interfaces
{
    public interface IPlayerFunction
    {
        Action<Player, VRRig> OnPlayerJoin { get; }
        Action<Player, VRRig> OnPlayerLeave { get; }
    }
}
