using System;

namespace GorillaInfoWatch.Models.Attributes;

[AttributeUsage(AttributeTargets.Class, Inherited = false, AllowMultiple = false)]
public class ShowOnHomeScreenAttribute : Attribute
{
    public string DisplayTitle;
}
