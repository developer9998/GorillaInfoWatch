using HarmonyLib;
using System;
using System.Reflection;

namespace GorillaInfoWatch.Models.Shortcuts;

public abstract class Shortcut
{
    public abstract string Name { get; }
    public abstract string Description { get; }
    public virtual bool State { get; } = false;

    internal bool HasState
    {
        get
        {
            Type type = GetType();
            PropertyInfo property = type.GetProperty(nameof(State), AccessTools.all);
            return property.DeclaringType == type;
        }
    }

    internal Assembly Assembly;

    public abstract void Invoke(bool isStateEnabled);

    internal string GetShortcutId()
    {
        if (Assembly == null) return Name;

        try
        {
            AssemblyName assemblyName = Assembly.GetName();
            return $"{assemblyName.Name}_{Name}";
        }
        catch (Exception)
        {

        }

        return Name;
    }
}