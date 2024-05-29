using System;

namespace GorillaInfoWatch.Interfaces
{
    public interface IQuickAction
    {
        bool? Active { get; }
        string Name { get; }
        Action Function { get; }
    }
}