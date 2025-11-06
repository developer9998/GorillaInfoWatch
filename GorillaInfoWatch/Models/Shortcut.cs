using System;
using System.Reflection;

namespace GorillaInfoWatch.Models
{
    public class Shortcut
    {
        public readonly string Name;

        public readonly string Description;

        public readonly bool IsToggleFunction;

        internal readonly Func<bool> StateGetter;

        internal readonly Action<bool> Method;

        internal Assembly CallingAssembly;

        internal string FunctionId => $"{CallingAssembly.GetName().Name}_{Name}";

        internal Shortcut(Assembly source, string name, string description, Action method)
        {
            CallingAssembly = source;

            Name = name;
            Description = description;
            Method = _ => method();
        }

        internal Shortcut(Assembly source, string name, string description, Func<bool> stateGetter, Action<bool> method)
        {
            CallingAssembly = source;

            Name = name;
            Description = description;

            IsToggleFunction = true;
            StateGetter = stateGetter;
            Method = method;
        }
    }
}
