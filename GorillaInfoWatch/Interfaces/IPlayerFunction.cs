using GorillaInfoWatch.Models;
using System;

namespace GorillaInfoWatch.Interfaces
{
    public interface IPlayerFunction
    {
        Action<PlayerArgs> OnPlayerJoin { get; }
        Action<PlayerArgs> OnPlayerLeave { get; }
    }
}
