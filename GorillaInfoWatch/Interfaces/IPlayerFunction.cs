using GorillaInfoWatch.Models;
using System;

namespace GorillaInfoWatch.Interfaces
{
    public interface IPlayerFunction
    {
        Action<PlayerInfo> OnPlayerJoin { get; }
        Action<PlayerInfo> OnPlayerLeave { get; }
    }
}
