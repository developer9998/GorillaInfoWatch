using System;
using System.Reflection;

namespace GorillaInfoWatch.Models.Shortcuts;

public class Shortcut
{
    public readonly string Name;

    public readonly string Description;

    public readonly ShortcutRestrictions Restrictions;

    public readonly bool HasState;

    internal readonly Func<bool> StateGetter;

    internal readonly Action<bool> Method;

    internal Assembly CallingAssembly;

    internal Shortcut(Assembly source, string name, string description, ShortcutRestrictions restrictions, Action method)
    {
        CallingAssembly = source;

        Name = name;
        Description = description;
        Restrictions = restrictions;

        Method = _ => method();
    }

    internal Shortcut(Assembly source, string name, string description, Func<bool> stateGetter, Action<bool> method)
    {
        CallingAssembly = source;

        Name = name;
        Description = description;

        HasState = true;
        StateGetter = stateGetter;
        Method = method;
    }

    internal string GetShortcutId()
    {
        if (CallingAssembly == null) return Name;

        try
        {
            AssemblyName assemblyName = CallingAssembly.GetName();
            return $"{assemblyName.Name}_{Name}_{(int)Restrictions}";
        }
        catch (Exception)
        {

        }

        return Name;
    }

    internal bool GetState()
    {
        if (!HasState || StateGetter == null) return false;

        try
        {
            bool state = StateGetter.Invoke();
            return state;
        }
        catch (Exception)
        {

        }

        return false;
    }
}