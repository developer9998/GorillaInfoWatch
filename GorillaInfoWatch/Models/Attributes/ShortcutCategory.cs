using GorillaInfoWatch.Models.Shortcuts;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace GorillaInfoWatch.Models.Attributes;

[AttributeUsage(AttributeTargets.Assembly, AllowMultiple = true, Inherited = false)]
public class ShortcutCategory(string title, params Type[] shortcuts) : Attribute
{
    public readonly string Title = title;

    public readonly Type[] ShortcutTypes = shortcuts;

    internal List<Shortcut> shortcuts;

    internal Assembly assembly;
}