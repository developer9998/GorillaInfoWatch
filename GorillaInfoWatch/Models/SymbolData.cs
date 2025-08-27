using System;

namespace GorillaInfoWatch.Models
{
    internal class SymbolData<T> where T : Enum
    {
        public static SymbolData<T> Shared { get; } = new SymbolData<T>();
    }
}
