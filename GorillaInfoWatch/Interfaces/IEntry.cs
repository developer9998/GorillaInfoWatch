using System;

namespace GorillaInfoWatch.Interfaces
{
    public interface IEntry
    {
        string Name { get; }
        Type Window { get; }
    }
}
