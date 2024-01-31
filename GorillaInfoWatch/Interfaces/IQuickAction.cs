using GorillaInfoWatch.Models;
using System;

namespace GorillaInfoWatch.Interfaces
{
    public interface IQuickAction
    {
        string Name { get; }
        ActionType Type { get; }

        bool InitialState { get; }

        Action<bool> OnActivate { get; }
    }
}