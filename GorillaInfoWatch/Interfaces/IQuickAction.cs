using System;

namespace GorillaInfoWatch.Interfaces
{
    public interface IQuickAction
    {
        string Name { get; }
        Action<bool> Function { get; }
    }
}